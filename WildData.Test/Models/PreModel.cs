using ModernRoute.WildData.Attributes;

namespace ModernRoute.WildData.Test.Models
{
    class PreModel
    {
        [Column("Field3", 255)]
        public string Property3
        {
            get;
            set;
        }

        [VolatileOnUpdate]
        [VolatileOnStore]
        public string Field18 = string.Empty;
    }
}
