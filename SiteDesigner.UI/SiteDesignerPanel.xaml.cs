using System.Windows.Controls;
using SiteDesigner.Core;

namespace SiteDesigner.UI
{
    public partial class SiteDesignerPanel : UserControl
    {
        private readonly SiteDesignerViewModel _vm = new SiteDesignerViewModel();

        public SiteDesignerPanel()
        {
            InitializeComponent();
            DataContext = _vm;
        }

        public void Bind(SiteConfig config) => _vm.LoadFrom(config);

        public void SaveTo(SiteConfig c)
        {
            c.SetbackFront = _vm.FrontSetback;
            c.SetbackSide = _vm.SideSetback;
            c.SetbackRear = _vm.RearSetback;
            c.StallWidth = _vm.StallWidth;
            c.StallDepth = _vm.StallDepth;
            c.AisleWidth = _vm.AisleWidth;
            c.TargetStalls = _vm.TargetStalls;
        }

        public SiteDesignerViewModel ViewModel => _vm;
    }
}
