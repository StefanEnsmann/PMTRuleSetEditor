using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gtk;

using PokemonTrackerEditor.Model;
using PokemonTrackerEditor.View;

namespace PokemonTrackerEditor {
    class MainProg {
        private Window mainWindow;

        private Entry locationFileName;
        private TreeView locationTreeView;
        private Dictionary<string, TreeView> locationConditionsTreeViews;

        private string currentFile;
        private RuleSet ruleSet;
        private DependencyEntry currentSelection;

        private void OnTreeSelectionChanged(object sender, EventArgs args) {
            currentSelection = null;
            if (((TreeSelection)sender).GetSelected(out TreeIter iter)) {
                object obj = ruleSet.Model.GetValue(iter, 0);
                TreeModelBaseClass entry = (TreeModelBaseClass)obj;
                if (entry != null) {
                    if (entry is LocationCategory locCat) {
                        currentSelection = locCat.Parent;
                    }
                    else if (entry is DependencyEntry depEntry) {
                        currentSelection = depEntry;
                    }
                }
            }
            UpdateEditorSelection(currentSelection);
        }

        private void LocationNameEdited(object sender, EditedArgs args) {
            TreePath path = new TreePath(args.Path);
            ruleSet.Model.GetIter(out TreeIter iter, path);
            TreeModelBaseClass data = (TreeModelBaseClass) ruleSet.Model.GetValue(iter, 0);
            if ((data is Location && ruleSet.LocationNameAvailable(args.NewText)) || (data is Check check && check.location.CheckNameAvailable(args.NewText, check.Type))) {
                data.Id = args.NewText;
                ruleSet.Model.Model.EmitRowChanged(path, iter);
            }
        }

        private void OnNewFileClick(object sender, EventArgs args) {
            if (ruleSet != null && ruleSet.HasChanged) {
                CustomDialog.ButtonType response = CustomDialog.YNCDialog(mainWindow, "There are changes to this file. Do you want to save?", "Save changes?");
                if (response == CustomDialog.ButtonType.YES) {
                    ruleSet.SaveToFile(currentFile);
                }
                if (response != CustomDialog.ButtonType.CANCEL) {
                    ruleSet.Cleanup();
                    ruleSet = new RuleSet();
                    currentFile = null;
                    locationFileName.Text = "Unsaved rule set";
                    locationTreeView.Model = ruleSet.Model;
                }
            }
            else {
                ruleSet = new RuleSet();
                currentFile = null;
                locationFileName.Text = "Unsaved rule set";
                locationTreeView.Model = ruleSet.Model;
            }
        }

        private void OnSelectFileClick(object sender, EventArgs args) {
            if (ruleSet != null && ruleSet.HasChanged) {
                CustomDialog.ButtonType response = CustomDialog.YNCDialog(mainWindow, "There are changes to this file. Do you want to save?", "Save changes?");
                if (response == CustomDialog.ButtonType.YES) {
                    ruleSet.SaveToFile(currentFile);
                }
                if (response != CustomDialog.ButtonType.CANCEL) {
                    ruleSet.Cleanup();
                    string selectedFile = CustomDialog.SelectRuleSetFile(mainWindow, "Select rule set file", true);
                    if (selectedFile != null) {
                        currentFile = selectedFile;
                        locationFileName.Text = currentFile;
                        ruleSet = RuleSet.FromFile(currentFile);
                        locationTreeView.Model = ruleSet.Model;
                    }
                }
            }
            else {
                if (ruleSet != null) {
                    ruleSet.Cleanup();
                }
                string selectedFile = CustomDialog.SelectRuleSetFile(mainWindow, "Select rule set file", true);
                if (selectedFile != null) {
                    currentFile = selectedFile;
                    locationFileName.Text = currentFile;
                    ruleSet = RuleSet.FromFile(currentFile);
                    locationTreeView.Model = ruleSet.Model;
                }
            }
        }

        private void OnAddElementClick(object sender, EventArgs args) {
            Button bt = (Button)sender;
            if (bt.Label.Equals("+ Location")) {
                string template = "new_location";
                int value = 0;
                while (!ruleSet.AddLocation(template + value)) {
                    ++value;
                }
            }
            else if (currentSelection != null) {
                Check.CheckType type;
                Location loc = currentSelection is Location location ? location : ((Check)currentSelection).location;
                switch (bt.Label) {
                    case "+ Item":
                        type = Check.CheckType.ITEM; break;
                    case "+ Pokémon":
                        type = Check.CheckType.POKEMON; break;
                    case "+ Trade":
                        type = Check.CheckType.TRADE; break;
                    case "+ Trainer":
                        type = Check.CheckType.TRAINER; break;
                    default:
                        return;
                }
                string template = "new_" + type.ToString().ToLower() + "_";
                int value = 0;
                while (!ruleSet.AddCheck(loc, template + value, type)) {
                    ++value;
                }
            }
        }

        private void OnAddConditionClick(object sender, EventArgs args) {
            if (currentSelection != null) {
                Button bt = (Button)sender;
                Check selectedCheck = CustomDialog.SelectCheck(mainWindow, "Select check", ruleSet.Model, Check.CheckType.ITEM);
                if (selectedCheck != null) {
                    currentSelection.AddCondition(selectedCheck);
                }
            }
        }

        private void OnSaveFileClick(object sender, EventArgs args) {
            if (ruleSet != null) {
                if (currentFile == null) {
                    string selectedFile = CustomDialog.SelectRuleSetFile(mainWindow, "Select file path", true);
                    if (selectedFile != null) {
                        currentFile = selectedFile;
                    }
                }
                if (currentFile != null) {
                    locationFileName.Text = currentFile;
                    ruleSet.SaveToFile(currentFile);
                }
                else {
                    CustomDialog.MessageDialog(mainWindow, "No file selected! Rule set was not saved.", "Warning");
                }
            }
        }

        private void UpdateEditorSelection(DependencyEntry entry) {
            if (entry != null) {
                Console.WriteLine("Activating editor: " + entry.ToString());
                locationConditionsTreeViews["Items"].Model = entry.ItemsModel;
                locationConditionsTreeViews["Pokémon"].Model = entry.PokemonModel;
                locationConditionsTreeViews["Trades"].Model = entry.TradesModel;
                locationConditionsTreeViews["Trainers"].Model = entry.TrainersModel;
                locationConditionsTreeViews["Story Items"].Model = entry.StoryItemsConditions.Model;
            }
            else {
                Console.WriteLine("Disabling editor");
            }
        }

        private void RenderDependencyEntryName(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            TreeModelBaseClass dep = (TreeModelBaseClass)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = dep.Id;
        }

        private void RenderDependencyEntryCheckCount(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            TreeModelBaseClass dep = (TreeModelBaseClass)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = dep is Location loc ? "" + loc.CheckCount : "";
        }

        private void RenderDependencyEntryDepCount(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            TreeModelBaseClass dep = (TreeModelBaseClass)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = dep is Check check ? "" + check.DependingEntryCount : "";
        }

        private void RenderDependencyEntryCondCount(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            TreeModelBaseClass dep = (TreeModelBaseClass)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = dep is DependencyEntry entry ? "" + entry.ConditionCount : "";
        }

        private void RenderConditionName(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            TreeModelBaseClass cond = (TreeModelBaseClass)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = cond.Id;
        }

        private void RenderConditionLocation(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            Check cond = (Check)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = cond.location.Id;
        }

        private void RenderStoryItemConditionName(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            StoryItemConditionBase storyItem = (StoryItemConditionBase)model.GetValue(iter, 0);
            (cell as CellRendererText).Text = storyItem.Id;
        }

        private void RenderStoryItemActive(TreeViewColumn column, CellRenderer cell, TreeModel model, TreeIter iter) {
            StoryItemConditionBase storyItem = (StoryItemConditionBase)model.GetValue(iter, 0);
            (cell as CellRendererToggle).Active = storyItem.Active;
        }

        private void InitUI() {
            // Main Window
            mainWindow = new Window("Pokémon Tracker Dependency Editor");
            mainWindow.DeleteEvent += delegate { Application.Quit(); };
            mainWindow.Resize(600, 600);

            VBox mainBox = new VBox();

            // File selection
            HBox locationFileBox = new HBox();
            locationFileName = new Entry { IsEditable = false, Text = "Unsaved rule set" };
            locationFileBox.PackStart(locationFileName, true, true, 0);

            Button locationFileSelectButton = new Button { Label = "Select location file" };
            locationFileSelectButton.Clicked += OnSelectFileClick;
            locationFileBox.PackStart(locationFileSelectButton, false, false, 0);

            Button locationFileNewButton = new Button { Label = "New file" };
            locationFileNewButton.Clicked += OnNewFileClick;
            locationFileBox.PackStart(locationFileNewButton, false, false, 0);

            Button locationFileSaveButton = new Button { Label = "Save file" };
            locationFileSaveButton.Clicked += OnSaveFileClick;
            locationFileBox.PackStart(locationFileSaveButton, false, false, 0);

            mainBox.PackStart(locationFileBox, false, false, 0);

            HPaned editorPaned = new HPaned();

            // Tree view
            VBox locationTreeBox = new VBox();
            ScrolledWindow locationTreeViewScrolledWindow = new ScrolledWindow();
            locationTreeView = new TreeView(ruleSet.Model);
            locationTreeViewScrolledWindow.Add(locationTreeView);

            TreeSelection locationTreeSelection = locationTreeView.Selection;
            locationTreeSelection.Changed += OnTreeSelectionChanged;

            TreeViewColumn locationNameColumn = new TreeViewColumn { Title = "Location", Resizable = true };

            CellRendererText locationNameColumnText = new CellRendererText { Editable = true };
            locationNameColumnText.Edited += LocationNameEdited;
            locationNameColumn.PackStart(locationNameColumnText, true);
            locationNameColumn.SetCellDataFunc(locationNameColumnText, new TreeCellDataFunc(RenderDependencyEntryName));

            locationTreeView.AppendColumn(locationNameColumn);

            TreeViewColumn locationCheckCountColumn = new TreeViewColumn() { Title = "# Checks", Resizable = true };
            CellRendererText locationCheckCountColumnText = new CellRendererText();
            locationCheckCountColumn.PackStart(locationCheckCountColumnText, false);
            locationCheckCountColumn.SetCellDataFunc(locationCheckCountColumnText, new TreeCellDataFunc(RenderDependencyEntryCheckCount));

            locationTreeView.AppendColumn(locationCheckCountColumn);

            TreeViewColumn locationDepCountColumn = new TreeViewColumn() { Title = "# Dependencies", Resizable = true };
            CellRendererText locationDepCountColumnText = new CellRendererText();
            locationDepCountColumn.PackStart(locationDepCountColumnText, false);
            locationDepCountColumn.SetCellDataFunc(locationDepCountColumnText, new TreeCellDataFunc(RenderDependencyEntryDepCount));

            locationTreeView.AppendColumn(locationDepCountColumn);

            TreeViewColumn locationCondCountColumn = new TreeViewColumn() { Title = "# Conditions", Resizable = true };
            CellRendererText locationCondCountColumnText = new CellRendererText();
            locationCondCountColumn.PackStart(locationCondCountColumnText, false);
            locationCondCountColumn.SetCellDataFunc(locationCondCountColumnText, new TreeCellDataFunc(RenderDependencyEntryCondCount));

            locationTreeView.AppendColumn(locationCondCountColumn);

            locationTreeBox.PackStart(locationTreeViewScrolledWindow, true, true, 0);

            HBox locationTreeControlsBox = new HBox();
            foreach (string type in new string[] { "+ Location", "+ Item", "+ Pokémon", "+ Trade", "+ Trainer"}) {
                Button addCheckButton = new Button { Label = type };
                addCheckButton.Clicked += OnAddElementClick;
                locationTreeControlsBox.PackStart(addCheckButton);
            }
            Button removeSelectedButton = new Button { Label = "Remove selected" };
            locationTreeControlsBox.PackStart(removeSelectedButton);

            locationTreeBox.PackStart(locationTreeControlsBox, false, false, 0);

            editorPaned.Add1(locationTreeBox);

            // Location editor
            Notebook locationConditionsNotebook = new Notebook();

            locationConditionsTreeViews = new Dictionary<string, TreeView>();
            foreach (string type in new string[] { "Items", "Pokémon", "Trades", "Trainers", "Story Items"}) {
                VBox condBox = new VBox();
                TreeView condTreeView = new TreeView();
                locationConditionsTreeViews[type] = condTreeView;

                if (!type.Equals("Story Items")) {
                    TreeViewColumn condLocationColumn = new TreeViewColumn { Title = "Location", Resizable = true };
                    CellRendererText condLocationColumnText = new CellRendererText();
                    condLocationColumn.PackStart(condLocationColumnText, true);
                    condLocationColumn.SetCellDataFunc(condLocationColumnText, new TreeCellDataFunc(RenderConditionLocation));
                    condTreeView.AppendColumn(condLocationColumn);

                    TreeViewColumn condCheckColumn = new TreeViewColumn { Title = "Check", Resizable = true };
                    CellRendererText condCheckColumnText = new CellRendererText();
                    condCheckColumn.PackStart(condCheckColumnText, true);
                    condCheckColumn.SetCellDataFunc(condCheckColumnText, new TreeCellDataFunc(RenderConditionName));
                    condTreeView.AppendColumn(condCheckColumn);
                }
                else {
                    TreeViewColumn condStoryItemColumn = new TreeViewColumn { Title = "Story Item", Resizable = true };
                    CellRendererText condStoryItemColumnText = new CellRendererText();
                    condStoryItemColumn.PackStart(condStoryItemColumnText, true);
                    condStoryItemColumn.SetCellDataFunc(condStoryItemColumnText, new TreeCellDataFunc(RenderStoryItemConditionName));
                    condTreeView.AppendColumn(condStoryItemColumn);

                    TreeViewColumn condStoryItemCheckedColumn = new TreeViewColumn { Title = "Necessary"};
                    CellRendererToggle condStoryItemCheckedColumnValue = new CellRendererToggle();
                    condStoryItemCheckedColumn.PackStart(condStoryItemCheckedColumnValue, true);
                    condStoryItemCheckedColumn.SetCellDataFunc(condStoryItemCheckedColumnValue, new TreeCellDataFunc(RenderStoryItemActive));
                    condTreeView.AppendColumn(condStoryItemCheckedColumn);
                }

                ScrolledWindow condTreeViewScrolledWindow = new ScrolledWindow { condTreeView };
                condBox.PackStart(condTreeViewScrolledWindow, true, true, 0);
                locationConditionsNotebook.AppendPage(condBox, new Label(type));
                if (!type.Equals("Story Items")) {
                    HBox condBoxControls = new HBox();
                    Button addCondition = new Button { Label = "Add condition" };
                    addCondition.Clicked += OnAddConditionClick;
                    condBoxControls.PackStart(addCondition);
                    Button removeCondition = new Button { Label = "Remove condition" };
                    condBoxControls.PackStart(removeCondition);
                    condBox.PackStart(condBoxControls, false, false, 0);
                }
            }

            editorPaned.Add2(locationConditionsNotebook);

            // Pack it all
            mainBox.PackStart(editorPaned, true, true, 0);

            mainWindow.Add(mainBox);

            mainWindow.ShowAll();
        }

        private void Run() {
            Application.Init();
            InitUI();
            Application.Run();
        }

        MainProg() {
            ruleSet = new RuleSet();
            currentFile = null;
        }

        static void Main(string[] args) {
            MainProg prog = new MainProg();
            prog.Run();
        }
    }
}
