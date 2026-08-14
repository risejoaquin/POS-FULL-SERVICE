using System.Collections.Generic;

namespace PosApplication.Models
{
    public class IndustryProfile
    {
        public required string IndustryName { get; set; }
        public required List<ShortcutConfig> Shortcuts { get; set; }
    }
}
