using System.Drawing;
using System.Windows.Forms;

namespace TorrentHandler
{
    public partial class ChoiceForm : Form
    {
        private const int ButtonWidth = 106;
        private const int ButtonHeight = 33;
        private const int MarginX = 12;
        private const int MarginY = 12;
        private const int SpacingX = 16;
        private const int SpacingY = 16;

        private readonly AppConfig _config;
        private readonly TorrentMetadata _metadata;

        public ChoiceForm(AppConfig config, TorrentMetadata metadata)
        {
            _config = config;
            _metadata = metadata;

            InitializeComponent();
            BuildButtons();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaptionText;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ChoiceForm";
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.Manual;
            Text = "TorrentHandler";
            TopMost = true;
            Location = new Point(509, 91);

            var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (icon != null)
            {
                Icon = icon;
            }

            ResumeLayout(false);
        }

        private void BuildButtons()
        {
            var categories = _config.Categories;
            var rows = (categories.Count + 1) / 2;

            var clientWidth = (MarginX * 2) + (ButtonWidth * 2) + SpacingX;
            var clientHeight = (MarginY * 2) + (rows * ButtonHeight) + ((rows - 1) * SpacingY);

            ClientSize = new Size(clientWidth, clientHeight);

            for (var i = 0; i < categories.Count; i++)
            {
                var category = categories[i];
                var row = i / 2;
                var isLeft = (i % 2) == 0;
                var y = MarginY + (row * (ButtonHeight + SpacingY));

                var button = new Button
                {
                    Font = new Font("Cambria", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0),
                    ImageAlign = ContentAlignment.MiddleRight,
                    Name = $"{category.Id}Button",
                    Size = new Size(ButtonWidth, ButtonHeight),
                    Text = category.Label,
                    UseVisualStyleBackColor = true
                };

                var isLastOdd = (categories.Count % 2 == 1) && (i == categories.Count - 1);
                if (isLastOdd)
                {
                    button.Location = new Point((ClientSize.Width - ButtonWidth) / 2, y);
                }
                else
                {
                    var x = isLeft ? MarginX : MarginX + ButtonWidth + SpacingX;
                    button.Location = new Point(x, y);
                }

                button.Click += (_, _) => OnCategoryClicked(category);
                Controls.Add(button);
            }
        }

        private void OnCategoryClicked(CategoryConfig category)
        {
            TorrentLauncher.Launch(category, _config, _metadata, isManualSelection: true);
            Close();
        }
    }
}
