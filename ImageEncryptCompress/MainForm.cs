using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.IO;
namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;
        RGBPixel[,] Imageenc;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
        }

        private void btnGaussSmooth_Click(object sender, EventArgs e)
        {
            double sigma = double.Parse(Seed.Text);
            int maskSize = (int)nudMaskSize.Value ;
            ImageMatrix = ImageOperations.GaussianFilter1D(ImageMatrix, maskSize, sigma);
            RGBPixel asd =ImageMatrix[1,1];
            txtWidth.Text=asd.red.ToString();
            txtHeight.Text =asd.green.ToString();
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int position = Convert.ToInt32(this.textBox2.Text);
            string seed = Seed.Text;
            BitArray intSeed = ImageOperations.convert(seed);
            ImageOperations.EncodeAndDecode(ref ImageMatrix, intSeed, position);
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
           
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int w1 = 0 , w2 = 0 , w3 = 0;
            node root_red = new node();
            node root_green = new node();
            node root_blue = new node();
            node cur = root_red;
            ImageOperations.tree(ref ImageMatrix, ref root_red, ref root_green, ref root_blue);
            using (StreamWriter writetext = new StreamWriter("tree.txt"))
            {   int accu=root_red.freq + root_green.freq + root_blue.freq;
                writetext.WriteLine("tree red");
                ImageOperations.print1(root_red,root_red , writetext);
                writetext.WriteLine("*****************************************" );
                writetext.WriteLine("tree green");
                ImageOperations.print1(root_green, root_green, writetext);
                writetext.WriteLine("*****************************************" );
                writetext.WriteLine("tree blue");
                ImageOperations.print1(root_blue, root_blue, writetext);
                writetext.WriteLine("*****************************************");
                 w1 = ImageOperations.ret_sum();
                 double ratio = (double) w1 / accu * 100;
                 writetext.WriteLine("number of byte after comprassion = " + w1);
                 writetext.WriteLine("number of byte befor comprassion = " + accu);
                 writetext.WriteLine("ratio = "+ratio+"%");
            }
            using (StreamWriter writetext = new StreamWriter("code_red.txt"))
            { ImageOperations.make_list(root_red, root_red, writetext);  }
            using (StreamWriter writetext = new StreamWriter("code_green.txt"))
            { ImageOperations.make_list(root_green, root_green, writetext);  }
            using (StreamWriter writetext = new StreamWriter("code_blue.txt"))
            { ImageOperations.make_list(root_blue, root_blue, writetext); } 
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (StreamWriter writetext = new StreamWriter("comprass.txt"))
            {
                ImageOperations.comprassion(ImageMatrix, writetext);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        { 
            int width;
            int high;
            StreamReader file =
            new StreamReader("width_high.txt");
                width =int.Parse( file.ReadLine());
                high =int.Parse( file.ReadLine());
                file.Close();
            RGBPixel[,] ImageMatrix1=new RGBPixel[width,high];
            node root_red = new node();
            node root_green = new node();
            node root_blue = new node();
            node cur = root_red;
            ImageOperations.tree(ref ImageMatrix, ref root_red, ref root_green, ref root_blue);
            ImageOperations.decomprassion1(ref ImageMatrix1, root_red, root_green, root_blue);
            ImageMatrix = ImageMatrix1;
            ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
        }

        private void button2_Click_1(object sender, EventArgs e)
        {   List<int> red_v = new List<int>();
            List<int> green_v = new List<int>();
            List<int> blue_v = new List<int>();
            List<string> red_code = new List<string>();
            List<string> green_code = new List<string>();
            List<string> blue_code = new List<string>();
            string line;
            StreamReader file =new StreamReader("code_red.txt");
            while ((line = file.ReadLine()) != null)
            {   red_v.Add(int.Parse(line));
                line = file.ReadLine();
                red_code.Add(line);                  }
            file.Close();
            StreamReader file1 =new StreamReader("code_green.txt");
            while ((line = file1.ReadLine()) != null)
            {   green_v.Add(int.Parse(line));
                line = file1.ReadLine();
                green_code.Add(line);                }
            file1.Close();
            StreamReader file2 = new StreamReader("code_blue.txt");
            while ((line = file2.ReadLine()) != null)
            {   blue_v.Add(int.Parse(line));
                line = file2.ReadLine();
                blue_code.Add(line);                 }
            file2.Close();
            int width , high;
            StreamReader file3 =new StreamReader("width_high.txt");
            width =int.Parse( file3.ReadLine());
            high =int.Parse( file3.ReadLine());
            file3.Close();
            RGBPixel[,] ImageMatrix1=new RGBPixel[width,high];
            ImageOperations.decomprassion2(ref ImageMatrix1, red_code, green_code, blue_code, red_v, green_v, blue_v);
            ImageMatrix = ImageMatrix1;
            ImageOperations.DisplayImage(ImageMatrix, pictureBox1);               
        }

      
   
    }
}