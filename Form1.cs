using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoCreateAIVideo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            var helper = new SheetsHelper();
            var prompts = await helper.GetInprocessPromptsAsync();

            foreach (var (row, prompt) in prompts)
            {
                Console.WriteLine($"Đang xử lý prompt dòng {row}: {prompt}");

                // Giả sử bạn tạo ảnh và video xong:
                string imageUrl = "https://drive.google.com/abc123";
                string videoUrl = "https://drive.google.com/xyz456";

                await helper.UpdatePromptRowAsync(row, imageUrl, videoUrl);
            }
        }
    }
}
