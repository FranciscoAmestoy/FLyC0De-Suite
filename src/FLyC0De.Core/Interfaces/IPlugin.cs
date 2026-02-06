using FLyC0De.Core.Models;

namespace FLyC0De.Core.Interfaces
{
    public interface IPlugin
    {
        string Id { get; }
        string Name { get; }
        string Description { get; }
        bool IsPaid { get; }
        PluginStatus Status { get; set; }
        string PurchaseUrl { get; }
        
        void Initialize();
    }
}
