namespace SiteDesigner.Core
{
    public class SiteConfig
    {
        public double SetbackFront { get; set; } = 25;
        public double SetbackSide { get; set; } = 10;
        public double SetbackRear { get; set; } = 20;
        public double StallWidth { get; set; } = 9;
        public double StallDepth { get; set; } = 18;
        public double AisleWidth { get; set; } = 24;
        public int TargetStalls { get; set; } = 40;
    }
}