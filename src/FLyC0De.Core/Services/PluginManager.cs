using System.Collections.Generic;
using FLyC0De.Core.Interfaces;
using FLyC0De.Core.Models;

namespace FLyC0De.Core.Services
{
    public class PluginManager
    {
        public List<IPlugin> Plugins { get; private set; } = new List<IPlugin>();

        public void LoadPlugins()
        {
            // 1. Foundational Tools
            Plugins.Add(new MockPlugin("set_stream_info", "Set Stream Info", "Update Title & Game simultaneously", true, PluginStatus.Locked));
            Plugins.Add(new MockPlugin("shoutout", "Shoutout", "Trigger shoutouts via Bot", true, PluginStatus.Locked));
            Plugins.Add(new MockPlugin("shoutout_player", "Shoutout Clip Player", "Play clips on shoutout (In Development)", true, PluginStatus.Inactive));

            // 2. Interactive
            Plugins.Add(new MockPlugin("poll_control", "Poll Control", "Manage stream polls", true, PluginStatus.Locked));
            Plugins.Add(new MockPlugin("subathon", "Subathon Controls", "Timer and goal management", true, PluginStatus.Locked));

            // 3. Expert (Last)
            Plugins.Add(new MockPlugin("super_macros", "Super Macros", "Basic macro functionality", false, PluginStatus.Active));
        }
    }

    public class MockPlugin : IPlugin
    {
        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public bool IsPaid { get; }
        public PluginStatus Status { get; set; }
        public string PurchaseUrl => "https://example.com/store/flyc0de";

        public MockPlugin(string id, string name, string desc, bool isPaid, PluginStatus status)
        {
            Id = id;
            Name = name;
            Description = desc;
            IsPaid = isPaid;
            Status = status;
        }

        public void Initialize() { }
    }
}
