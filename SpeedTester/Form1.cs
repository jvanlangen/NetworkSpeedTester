namespace SpeedTester
{
    public partial class FormSpeedTester : Form
    {
        private readonly HostSearcher _hostSearcher;

        public FormSpeedTester()
        {
            InitializeComponent();

            _hostSearcher = new();
        }
    }
}