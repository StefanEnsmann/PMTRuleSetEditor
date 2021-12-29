using Gtk;

using PokemonTrackerEditor.Model;

using System;
using System.Collections.Generic;

namespace PokemonTrackerEditor.View.MainWindow {
    class MainWindow : Window {
        public MainProg Main { get; private set; }
        public DependencyEntry CurrentLocationSelection { get; set; }
        public StoryItemBase CurrentStoryItemSelection { get; set; }

        private string windowTitle = "Pokémon Map Tracker Rule Set Editor";
        private TreeView locationTreeView;
        private TreeView locationLocalizationTreeView;
        private TreeView storyItemsTreeView;
        private TreeView storyItemsLocalizationTreeView;
        private Entry storyItemsURLEntry;
        public TreeView pokedexTreeView { get; private set; }

        private Dictionary<string, TreeView> locationConditionsTreeViews;

        public void SetRuleSet(RuleSet ruleSet, string filename) {
            SetRuleSetPath(filename);
            locationTreeView.Model = ruleSet.Model;
            locationTreeView.Selection.SelectIter(TreeIter.Zero);
            storyItemsTreeView.Model = ruleSet.StoryItems.Model;
            storyItemsTreeView.Selection.SelectIter(TreeIter.Zero);
            CurrentLocationSelection = null;
            CurrentStoryItemSelection = null;
        }

        public void SetRuleSetPath(string filename) {
            Title = windowTitle + ": " + (filename ?? "Unsaved rule set");
        }

        public void UpdateLocationEditorSelection(DependencyEntry entry) {
            CurrentLocationSelection = entry;
            if (entry != null) {
                locationLocalizationTreeView.Model = entry.Localization.Model;
                locationConditionsTreeViews["Items"].Model = entry.ItemsModel;
                locationConditionsTreeViews["Pokémon"].Model = entry.PokemonModel;
                locationConditionsTreeViews["Trades"].Model = entry.TradesModel;
                locationConditionsTreeViews["Trainers"].Model = entry.TrainersModel;
                locationConditionsTreeViews["Story Items"].Model = entry.StoryItemsConditions.Model;
            }
            else {
                foreach (TreeView treeView in locationConditionsTreeViews.Values) {
                    treeView.Model = null;
                }
            }
        }

        public void UpdateStoryItemEditorSelection(StoryItemBase storyItemBase) {
            CurrentStoryItemSelection = storyItemBase;
            storyItemsURLEntry.IsEditable = false;
            storyItemsURLEntry.Text = "";
            if (storyItemBase != null) {
                storyItemsLocalizationTreeView.Model = storyItemBase.Localization.Model;
                if (storyItemBase is StoryItem) {
                    storyItemsURLEntry.IsEditable = true;
                    storyItemsURLEntry.Text = (storyItemBase as StoryItem).ImageURL;
                }
            }
            else {
                storyItemsLocalizationTreeView.Model = null;
            }
        }

        private CheckButton CreateLanguageCheckButton(string languageCode, string language) {
            CheckButton chkBtn = new CheckButton(language);
            return chkBtn;
        }

        private Frame CreateCheckConditionList(string title, Check.CheckType type) {
            Frame frame = new Frame(title);
            VBox condBox = new VBox { Spacing = 5 };

            TreeView condTreeView = new TreeView();
            locationConditionsTreeViews[title] = condTreeView;

            TreeViewColumn condLocationColumn = new TreeViewColumn { Title = "Location", Resizable = true };
            CellRendererText condLocationColumnText = new CellRendererText();
            condLocationColumn.PackStart(condLocationColumnText, true);
            condLocationColumn.SetCellDataFunc(condLocationColumnText, new TreeCellDataFunc(Renderers.ConditionLocation));
            condTreeView.AppendColumn(condLocationColumn);

            TreeViewColumn condCheckColumn = new TreeViewColumn { Title = "Check", Resizable = true };
            CellRendererText condCheckColumnText = new CellRendererText();
            condCheckColumn.PackStart(condCheckColumnText, true);
            condCheckColumn.SetCellDataFunc(condCheckColumnText, new TreeCellDataFunc(Renderers.ConditionName));
            condTreeView.AppendColumn(condCheckColumn);

            ScrolledWindow condTreeViewScrolledWindow = new ScrolledWindow { condTreeView };
            condBox.PackStart(condTreeViewScrolledWindow, true, true, 0);

            Toolbar condBoxControls = new Toolbar();
            ToolButton addCondition = new ToolButton(Stock.Add);
            addCondition.Clicked += (object sender, EventArgs args) => { ButtonCallbacks.OnAddConditionClick(this, type); };
            condBoxControls.Insert(addCondition, 0);
            ToolButton removeCondition = new ToolButton(Stock.Remove);
            removeCondition.Clicked += (object sender, EventArgs args) => { ButtonCallbacks.OnRemoveConditionClick(this, condTreeView); };
            condBoxControls.Insert(removeCondition, 1);

            condBox.PackStart(condBoxControls, false, false, 0);

            frame.Add(condBox);
            return frame;
        }

        private TreeViewColumn CreateLocationColumn(string title, TreeCellDataFunc func) {
            TreeViewColumn column = new TreeViewColumn() { Title = title, Resizable = true };
            CellRendererText columnText = new CellRendererText();
            column.PackStart(columnText, false);
            column.SetCellDataFunc(columnText, func);
            return column;
        }

        private ToolButton CreateFileButton(string title, string stock_id, EventHandler func) {
            ToolButton fileButton = new ToolButton(stock_id) { Label = title };
            fileButton.Clicked += func;
            return fileButton;
        }

        private ToolButton CreateCheckButton(string title, Check.CheckType type) {
            ToolButton addCheckButton = new ToolButton(Stock.Add) { Label = title };
            addCheckButton.Clicked += (object sender, EventArgs args) => { ButtonCallbacks.OnAddCheckClick(this, type); };
            return addCheckButton;
        }

        public MainWindow(MainProg main) : base("") {
            int windowWidth = 1000;
            int windowHeight = 600;
            Main = main;
            windowTitle = "Pokémon Map Tracker Rule Set Editor";
            SetRuleSetPath(null);


            DeleteEvent += delegate { Application.Quit(); };
            Resize(windowWidth, windowHeight);

            VBox mainBox = new VBox { Spacing = 5 };

            // File selection
            Toolbar locationFileBox = new Toolbar();
            locationFileBox.Insert(CreateFileButton("New", Stock.New, (object sender, EventArgs args) => { ButtonCallbacks.OnNewFileClick(this); }), 0);
            locationFileBox.Insert(CreateFileButton("Open", Stock.Open, (object sender, EventArgs args) => { ButtonCallbacks.OnSelectFileClick(this); }), 1);
            locationFileBox.Insert(CreateFileButton("Save", Stock.Save, (object sender, EventArgs args) => { ButtonCallbacks.OnSaveFileClick(this); }), 2);

            mainBox.PackStart(locationFileBox, false, false, 0);

            Notebook mainNotebook = new Notebook();

            // General view
            VBox generalBox = new VBox { Spacing = 5 };

            Frame rulesetNameFrame = new Frame("Ruleset Name");
            Entry rulesetNameEntry = new Entry();
            rulesetNameEntry.Changed += (object sender, EventArgs args) => { Main.RuleSet.SetName(((Entry)sender).Text); };
            rulesetNameFrame.Add(rulesetNameEntry);
            generalBox.PackStart(rulesetNameFrame, false, false, 0);

            Frame gameFrame = new Frame("Game");
            ListStore gameModel = new ListStore(typeof(string));
            foreach (string game in Main.DefaultGames) {
                gameModel.AppendValues(game);
            }
            ComboBoxEntry gameNameComboBox = ComboBoxEntry.NewText();
            gameNameComboBox.Model = gameModel;
            gameNameComboBox.Changed += (object sender, EventArgs args) => { Main.RuleSet.SetGame(((ComboBox)sender).ActiveText); };
            gameFrame.Add(gameNameComboBox);

            generalBox.PackStart(gameFrame, false, false, 0);

            Frame languagesFrame = new Frame("Languages");
            Table languagesTable = new Table(3, 3, false);
            uint row = 0, col = 0;
            foreach (KeyValuePair<string, string> languages in main.SupportedLanguages) {
                CheckButton chkBtn = CreateLanguageCheckButton(languages.Key, languages.Value);
                chkBtn.Toggled += (object sender, EventArgs args) => { Main.RuleSet.SetLanguageActive(languages.Key, ((CheckButton)sender).Active); };
                languagesTable.Attach(chkBtn, col, col + 1, row, row + 1);
                col = (col + 1) % 3;
                if (col == 0) {
                    row = (row + 1) % 3;
                }
            }
            languagesFrame.Add(languagesTable);

            generalBox.PackStart(languagesFrame, false, false, 0);

            mainNotebook.AppendPage(generalBox, new Label { Text = "General" });

            HPaned editorPaned = new HPaned();

            // Locations view
            VBox locationTreeBox = new VBox { Spacing = 5 };
            ScrolledWindow locationTreeViewScrolledWindow = new ScrolledWindow();
            locationTreeView = new TreeView(main.RuleSet.Model);

            locationTreeView.Selection.Changed += (object sender, EventArgs args) => { EventCallbacks.OnLocationTreeSelectionChanged(this, (TreeSelection)sender); };

            TreeViewColumn locationNameColumn = new TreeViewColumn { Title = "Location", Resizable = true };

            CellRendererText locationNameColumnText = new CellRendererText { Editable = true };
            locationNameColumnText.Edited += (object sender, EditedArgs args) => { EventCallbacks.OnLocationNameEdited(this, new TreePath(args.Path), args.NewText); };
            locationNameColumn.PackStart(locationNameColumnText, true);
            locationNameColumn.SetCellDataFunc(locationNameColumnText, new TreeCellDataFunc(Renderers.DependencyEntryName));

            locationTreeView.AppendColumn(locationNameColumn);
            locationTreeView.AppendColumn(CreateLocationColumn("# Checks", new TreeCellDataFunc(Renderers.DependencyEntryCheckCount)));
            locationTreeView.AppendColumn(CreateLocationColumn("# Dependencies", new TreeCellDataFunc(Renderers.DependencyEntryDepCount)));
            locationTreeView.AppendColumn(CreateLocationColumn("# Conditions", new TreeCellDataFunc(Renderers.DependencyEntryCondCount)));
            locationTreeViewScrolledWindow.Add(locationTreeView);

            locationTreeBox.PackStart(locationTreeViewScrolledWindow, true, true, 0);

            // Location controls
            Toolbar locationTreeToolbar = new Toolbar();
            ToolButton addCheckButton = new ToolButton(Stock.Add) { Label = "Location" };
            addCheckButton.Clicked += (object sender, EventArgs args) => { ButtonCallbacks.OnAddLocationClick(this); };
            locationTreeToolbar.Insert(addCheckButton, 0);
            locationTreeToolbar.Insert(CreateCheckButton("Item", Check.CheckType.ITEM), 1);
            locationTreeToolbar.Insert(CreateCheckButton("Pokémon", Check.CheckType.POKEMON), 2);
            locationTreeToolbar.Insert(CreateCheckButton("Trade", Check.CheckType.TRADE), 3);
            locationTreeToolbar.Insert(CreateCheckButton("Trainer", Check.CheckType.TRAINER), 4);

            ToolButton removeSelectedButton = new ToolButton(Stock.Remove) { Label = "Remove" };
            removeSelectedButton.Clicked += (object sender, EventArgs args) => { ButtonCallbacks.OnRemoveLocationOrCheckClick(this); };
            locationTreeToolbar.Insert(removeSelectedButton, 5);

            locationTreeBox.PackStart(locationTreeToolbar, false, false, 0);

            editorPaned.Add1(locationTreeBox);

            // Location editor
            locationConditionsTreeViews = new Dictionary<string, TreeView>();
            Table locationEditorTable = new Table(3, 2, true);

            Frame locationLocalizationFrame = new Frame("Localization");
            locationLocalizationTreeView = new TreeView();
            TreeViewColumn locationLocalizationCodeColumn = new TreeViewColumn { Title = "Language" };
            CellRendererText locationLocalizationCodeColumnText = new CellRendererText();
            locationLocalizationCodeColumn.PackStart(locationLocalizationCodeColumnText, true);
            locationLocalizationCodeColumn.SetCellDataFunc(locationLocalizationCodeColumnText, new TreeCellDataFunc(Renderers.LocalizationCodeCell));
            locationLocalizationTreeView.AppendColumn(locationLocalizationCodeColumn);

            TreeViewColumn locationLocalizationValueColumn = new TreeViewColumn { Title = "Value", Resizable = true };
            CellRendererText locationLocalizationValueColumnText = new CellRendererText { Editable = true };
            locationLocalizationValueColumnText.Edited += (object sender, EditedArgs args) => { EventCallbacks.OnLocalizationValueEdited(this, new TreePath(args.Path), args.NewText); };
            locationLocalizationValueColumn.PackStart(locationLocalizationValueColumnText, true);
            locationLocalizationValueColumn.SetCellDataFunc(locationLocalizationValueColumnText, new TreeCellDataFunc(Renderers.LocalizationValueCell));
            locationLocalizationTreeView.AppendColumn(locationLocalizationValueColumn);

            locationLocalizationFrame.Add(locationLocalizationTreeView);
            locationEditorTable.Attach(locationLocalizationFrame, 0, 1, 0, 1);

            locationEditorTable.Attach(CreateCheckConditionList("Items", Check.CheckType.ITEM), 1, 2, 0, 1);
            locationEditorTable.Attach(CreateCheckConditionList("Pokémon", Check.CheckType.POKEMON), 0, 1, 1, 2);
            locationEditorTable.Attach(CreateCheckConditionList("Trades", Check.CheckType.TRADE), 1, 2, 1, 2);
            locationEditorTable.Attach(CreateCheckConditionList("Trainers", Check.CheckType.TRAINER), 0, 1, 2, 3);

            Frame storyItemConditionFrame = new Frame("Story Items");
            VBox condStoryItemBox = new VBox { Spacing = 5 };

            TreeView condStoryItemTreeView = new TreeView();
            locationConditionsTreeViews["Story Items"] = condStoryItemTreeView;

            TreeViewColumn condStoryItemColumn = new TreeViewColumn { Title = "Item", Resizable = true };
            CellRendererText condStoryItemColumnText = new CellRendererText();
            condStoryItemColumn.PackStart(condStoryItemColumnText, true);
            condStoryItemColumn.SetCellDataFunc(condStoryItemColumnText, new TreeCellDataFunc(Renderers.StoryItemConditionName));
            condStoryItemTreeView.AppendColumn(condStoryItemColumn);

            TreeViewColumn condStoryItemCountColumn = new TreeViewColumn { Title = "Count", Resizable = true };
            CellRendererText condStoryItemCountColumnText = new CellRendererText();
            condStoryItemCountColumn.PackStart(condStoryItemCountColumnText, true);
            condStoryItemCountColumn.SetCellDataFunc(condStoryItemCountColumnText, new TreeCellDataFunc(Renderers.StoryItemConditionCollectionCount));
            condStoryItemTreeView.AppendColumn(condStoryItemCountColumn);

            ScrolledWindow condStoryItemTreeViewScrolledWindow = new ScrolledWindow { condStoryItemTreeView };
            condStoryItemBox.PackStart(condStoryItemTreeViewScrolledWindow, true, true, 0);

            Toolbar condStoryItemBoxControls = new Toolbar();
            ToolButton addStoryItemCondition = new ToolButton(Stock.Add) { Label = "Item" };
            addStoryItemCondition.Clicked += (object sender, EventArgs args) => { ButtonCallbacks.OnAddStoryItemConditionClick(this, condStoryItemTreeView); };
            condStoryItemBoxControls.Insert(addStoryItemCondition, 0);
            ToolButton addStoryItemANDCollection = new ToolButton(Stock.Add) { Label = "AND" };
            addStoryItemANDCollection.Clicked += (object sender, EventArgs args) => { ButtonCallbacks.OnAddStoryItemConditionANDCollectionClick(this, condStoryItemTreeView); };
            condStoryItemBoxControls.Insert(addStoryItemANDCollection, 1);
            ToolButton addStoryItemORCollection = new ToolButton(Stock.Add) { Label = "OR" };
            addStoryItemORCollection.Clicked += (object sender, EventArgs args) => { ButtonCallbacks.OnAddStoryItemConditionORCollectionClick(this, condStoryItemTreeView); };
            condStoryItemBoxControls.Insert(addStoryItemORCollection, 2);
            ToolButton removeStoryItemCondition = new ToolButton(Stock.Remove);
            removeStoryItemCondition.Clicked += (object sender, EventArgs args) => { ButtonCallbacks.OnRemoveStoryItemConditionClick(this, condStoryItemTreeView); };
            condStoryItemBoxControls.Insert(removeStoryItemCondition, 3);

            condStoryItemBox.PackStart(condStoryItemBoxControls, false, false, 0);

            storyItemConditionFrame.Add(condStoryItemBox);

            locationEditorTable.Attach(storyItemConditionFrame, 1, 2, 2, 3);

            editorPaned.Add2(locationEditorTable);
            editorPaned.Position = (int)(windowWidth * 0.4);

            mainNotebook.AppendPage(editorPaned, new Label { Text = "Locations" });

            // Story Items
            HPaned storyItemsPaned = new HPaned();
            VBox storyItemsBox = new VBox { Spacing = 5 };
            ScrolledWindow storyItemsScrolledWindow = new ScrolledWindow();
            storyItemsTreeView = new TreeView(main.RuleSet.StoryItems.Model);
            storyItemsTreeView.Selection.Changed += (object sender, EventArgs args) => { EventCallbacks.OnStoryItemTreeSelectionChanged(this, (TreeSelection)sender); };

            TreeViewColumn storyItemsNameColumn = new TreeViewColumn { Title = "Story Item", Resizable = true };
            CellRendererText storyItemsNameColumnText = new CellRendererText { Editable = true };
            storyItemsNameColumnText.Edited += (object sender, EditedArgs args) => { EventCallbacks.OnStoryItemNameEdited(this, new TreePath(args.Path), args.NewText); };
            storyItemsNameColumn.PackStart(storyItemsNameColumnText, true);
            storyItemsNameColumn.SetCellDataFunc(storyItemsNameColumnText, new TreeCellDataFunc(Renderers.StoryItemName));
            storyItemsTreeView.AppendColumn(storyItemsNameColumn);

            TreeViewColumn storyItemsDependenciesColumn = new TreeViewColumn { Title = "Dependencies", Resizable = true };
            CellRendererText storyItemsDependenciesColumnText = new CellRendererText();
            storyItemsDependenciesColumn.PackStart(storyItemsDependenciesColumnText, true);
            storyItemsDependenciesColumn.SetCellDataFunc(storyItemsDependenciesColumnText, new TreeCellDataFunc(Renderers.StoryItemDependencyCount));
            storyItemsTreeView.AppendColumn(storyItemsDependenciesColumn);

            storyItemsScrolledWindow.Add(storyItemsTreeView);
            storyItemsBox.PackStart(storyItemsScrolledWindow, true, true, 0);

            Toolbar storyItemsToolbar = new Toolbar();
            ToolButton addCategoryButton = new ToolButton(Stock.Add) { Label = "Category" };
            addCategoryButton.Clicked += (object sender, EventArgs args) => { ButtonCallbacks.OnAddStoryItemCategoryClick(this); };
            storyItemsToolbar.Insert(addCategoryButton, 0);
            ToolButton addStoryItemButton = new ToolButton(Stock.Add) { Label = "Story Item" };
            addStoryItemButton.Clicked += (object sender, EventArgs args) => { ButtonCallbacks.OnAddStoryItemClick(this); };
            storyItemsToolbar.Insert(addStoryItemButton, 1);
            ToolButton removeSelectedStoryItemButton = new ToolButton(Stock.Remove) { Label = "Remove" };
            removeSelectedStoryItemButton.Clicked += (object sender, EventArgs args) => { ButtonCallbacks.OnRemoveStoryItemClick(this); };
            storyItemsToolbar.Insert(removeSelectedStoryItemButton, 2);
            ToolButton moveUpButton = new ToolButton(Stock.GoUp) { Label = "Move Up" };
            moveUpButton.Clicked += (object sender, EventArgs args) => { ButtonCallbacks.OnMoveUpStoryItemClick(this); };
            storyItemsToolbar.Insert(moveUpButton, 3);
            ToolButton moveDownButton = new ToolButton(Stock.GoDown) { Label = "Move Down" };
            moveDownButton.Clicked += (object sender, EventArgs args) => { ButtonCallbacks.OnMoveDownStoryItemClick(this); };
            storyItemsToolbar.Insert(moveDownButton, 4);

            storyItemsBox.PackStart(storyItemsToolbar, false, false, 0);
            storyItemsPaned.Add1(storyItemsBox);

            VBox storyItemsEditor = new VBox { Spacing = 5 };
            Frame storyItemsLocalizationFrame = new Frame("Localization");
            storyItemsEditor.PackStart(storyItemsLocalizationFrame, true, true, 0);
            storyItemsLocalizationTreeView = new TreeView();
            TreeViewColumn storyItemsLocalizationCodeColumn = new TreeViewColumn { Title = "Language" };
            CellRendererText storyItemsLocalizationCodeColumnText = new CellRendererText();
            storyItemsLocalizationCodeColumn.PackStart(storyItemsLocalizationCodeColumnText, true);
            storyItemsLocalizationCodeColumn.SetCellDataFunc(storyItemsLocalizationCodeColumnText, new TreeCellDataFunc(Renderers.LocalizationCodeCell));
            storyItemsLocalizationTreeView.AppendColumn(storyItemsLocalizationCodeColumn);

            TreeViewColumn storyItemsLocalizationValueColumn = new TreeViewColumn { Title = "Value", Resizable = true };
            CellRendererText storyItemsLocalizationValueColumnText = new CellRendererText { Editable = true };
            storyItemsLocalizationValueColumnText.Edited += (object sender, EditedArgs args) => { EventCallbacks.OnLocalizationValueEdited(this, new TreePath(args.Path), args.NewText, false); };
            storyItemsLocalizationValueColumn.PackStart(storyItemsLocalizationValueColumnText, true);
            storyItemsLocalizationValueColumn.SetCellDataFunc(storyItemsLocalizationValueColumnText, new TreeCellDataFunc(Renderers.LocalizationValueCell));
            storyItemsLocalizationTreeView.AppendColumn(storyItemsLocalizationValueColumn);

            storyItemsLocalizationFrame.Add(storyItemsLocalizationTreeView);

            Frame storyItemsURLFrame = new Frame("Image URL");
            storyItemsURLEntry = new Entry() { IsEditable = false };
            storyItemsURLEntry.Changed += (object sender, EventArgs args) => { EventCallbacks.OnStoryItemURLEdited(this, storyItemsURLEntry.Text); };
            storyItemsURLFrame.Add(storyItemsURLEntry);
            storyItemsEditor.PackStart(storyItemsURLFrame, false, false, 0);

            storyItemsPaned.Add2(storyItemsEditor);

            storyItemsPaned.Position = (int)(windowWidth * 0.7);

            mainNotebook.AppendPage(storyItemsPaned, new Label { Text = "Story Items" });

            // Pokedex
            VBox pokedexBox = new VBox { Spacing = 5 };
            ScrolledWindow pokedexScrolledWindow = new ScrolledWindow();
            pokedexTreeView = new TreeView(Main.Pokedex.ListStore);

            TreeViewColumn pokedexIdxColumn = new TreeViewColumn { Title = "#", Resizable = true };
            CellRendererText pokedexIdxColumnText = new CellRendererText();
            pokedexIdxColumn.PackStart(pokedexIdxColumnText, true);
            pokedexIdxColumn.SetCellDataFunc(pokedexIdxColumnText, new TreeCellDataFunc(Renderers.PokedexIndexCell));
            pokedexTreeView.AppendColumn(pokedexIdxColumn);

            TreeViewColumn pokedexNameColumn = new TreeViewColumn { Title = "Name", Resizable = true };
            CellRendererText pokedexNameColumnText = new CellRendererText();
            pokedexNameColumn.PackStart(pokedexNameColumnText, true);
            pokedexNameColumn.SetCellDataFunc(pokedexNameColumnText, new TreeCellDataFunc(Renderers.PokedexNameCell));
            pokedexTreeView.AppendColumn(pokedexNameColumn);

            TreeViewColumn pokedexAvailableColumn = new TreeViewColumn { Title = "Available", Resizable = true };
            CellRendererToggle pokedexAvailableColumnBox = new CellRendererToggle() { Activatable = true };
            pokedexAvailableColumnBox.Toggled += (object sender, ToggledArgs args) => { EventCallbacks.OnPokedexEntryToggled(this, new TreePath(args.Path)); };
            pokedexAvailableColumnBox.Mode = CellRendererMode.Activatable;
            pokedexAvailableColumn.PackStart(pokedexAvailableColumnBox, true);
            pokedexAvailableColumn.SetCellDataFunc(pokedexAvailableColumnBox, new TreeCellDataFunc(Renderers.PokedexAvailableCell));
            pokedexTreeView.AppendColumn(pokedexAvailableColumn);

            pokedexScrolledWindow.Add(pokedexTreeView);
            pokedexBox.PackStart(pokedexScrolledWindow, true, true, 0);

            Toolbar pokedexToolbar = new Toolbar();
            ToolButton applyTemplateButton = new ToolButton(Stock.Delete) { Label = "Clear" };
            applyTemplateButton.Clicked += (object sender, EventArgs args) => { ButtonCallbacks.OnApplyPokedexTemplateClick(this, null); };
            pokedexToolbar.Insert(applyTemplateButton, 0);
            int idx = 1;
            foreach (string pokedexTemplate in main.Pokedex.Templates.Keys) {
                applyTemplateButton = new ToolButton(Stock.Apply) { Label = pokedexTemplate };
                applyTemplateButton.Clicked += (object sender, EventArgs args) => { ButtonCallbacks.OnApplyPokedexTemplateClick(this, pokedexTemplate); };
                pokedexToolbar.Insert(applyTemplateButton, idx++);
            }
            pokedexBox.PackStart(pokedexToolbar, false, false, 0);
            mainNotebook.AppendPage(pokedexBox, new Label { Text = "Pokédex" });

            // Pack it all
            mainBox.PackStart(mainNotebook, true, true, 0);

            Add(mainBox);

            ShowAll();
        }
    }
}
