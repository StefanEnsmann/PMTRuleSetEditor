using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonTrackerEditor.Model {
    public interface ILocationContainer : IMovableItems<DependencyEntry> {
        List<Location> Locations { get; }
        RuleSet Rules { get; }

        void ChildWasRemoved(DependencyEntry entry);
    }

    public static class LocationContainerExtensions {
        public static string LocationPath(this ILocationContainer container) {
            if (container is RuleSet) {
                return "";
            }
            else if (container is Location location) {
                string parentPath = location.Parent.LocationPath();
                return  parentPath + (parentPath.Length > 0 ? "." : "") + location.Id;
            }
            else {
                return null;
            }
        }

        public static Location AddLocation(this ILocationContainer container, string location) {
            if (container.LocationNameAvailable(location)) {
                Location loc = new Location(location, container);
                container.Locations.Add(loc);
                container.Rules.MergeLocationToModel(loc);
                container.Rules.ReportChange();
                return loc;
            }
            return null;
        }

        public static bool LocationNameAvailable(this ILocationContainer container, string locationName) {
            if (!(new string[] { "items", "pokemon", "trades", "trainers" }).Contains(locationName)) {
                foreach (Location location in container.Locations) {
                    if (location.Id.Equals(locationName)) {
                        return false;
                    }
                }
                return true;
            }
            else {
                return false;
            }
        }

        public static DependencyEntry ResolvePath(this ILocationContainer container, string path) {
            return container.ResolvePath(path.Split('.').ToList());
        }

        public static DependencyEntry ResolvePath(this ILocationContainer container, List<string> path) {
            if (path.Count == 0) {
                return container as DependencyEntry;
            }
            else {
                string segment = path.First();
                path.RemoveAt(0);
                if (container is Location location) {
                    List<Check> checks;
                    switch (segment) {
                        case "items":
                            checks = location.Items;
                            break;
                        case "pokemon":
                            checks = location.Pokemon;
                            break;
                        case "trades":
                            checks = location.Trades;
                            break;
                        case "trainers":
                            checks = location.Trainers;
                            break;
                        default:
                            checks = null;
                            break;
                    }
                    if (checks != null) {
                        segment = path.First();
                        foreach (Check check in checks) {
                            if (check.Id.Equals(segment)) {
                                return check;
                            }
                        }
                    }
                }
                foreach (Location loc in container.Locations) {
                    if (loc.Id.Equals(segment)) {
                        return loc.ResolvePath(path);
                    }
                }
                return null;
            }
        }
    }
}
