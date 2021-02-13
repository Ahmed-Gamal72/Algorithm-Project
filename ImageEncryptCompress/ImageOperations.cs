using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
namespace ImageQuantization
{
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }
    class comprass
    {
        public List<List<bool> > ob;
        BinaryFormatter formatter;
        string fileName;

        public comprass(string fileName)
        {
            
            ob = new List<List<bool>>();
            
            this.fileName = fileName;
            formatter = new BinaryFormatter();

        }


        public void load()
        {
            ob.Clear();
            FileStream stream = new FileStream(fileName, FileMode.Open);
            ob = (List<List<bool>>)formatter.Deserialize(stream);
            stream.Close();
        }


        public void save()
        {
            FileStream stream = new FileStream(fileName, FileMode.Append);
            formatter.Serialize(stream, ob);
            stream.Close();
        }
    }
    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
       
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        static string lll;
        static int sumtion=0;
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;
            RGBPixel[,] Buffer = new RGBPixel[Height, Width];
            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }
        
        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }
       /// <summary>
       /// Apply Gaussian smoothing filter to enhance the edge detection 
       /// </summary>
       /// <param name="ImageMatrix">Colored image matrix</param>
       /// <param name="filterSize">Gaussian mask size</param>
       /// <param name="sigma">Gaussian sigma</param>
       /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];

           
            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                    
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }
           
                return Filtered;
        }
 //########################################################################################################################################################
        public static BitArray convert(string e)
        {   int xc = e.Length - 1 , i = 0;
            BitArray v = new BitArray(1000);
            while (xc >= 0)
            {    if (e[xc] == '1')
                {  v[i] = true;      i++;  }
                 else if (e[xc] == '0')
                {  v[i] = false;  i++;    }
                // Bouns convert integer to binary
                else
                {   byte n = (byte)e[xc];
                    BitArray t = new BitArray(new byte[] { n });
                    for (int ii = 0; ii < 8; ii++)
                    {  v[i] = t[ii];   i++;  }
                }
                xc--;
            }
            BitArray rt = new BitArray(i);
            for (int k = 0; k < i; k++)  rt[k] = v[k];
            return rt;
        } 
        public static void EncodeAndDecode(ref RGBPixel[,] ImageMatrix, BitArray seedInBit, int Tap)
        {
            int length = seedInBit.Length - 1;
            int[] valueInDecimal = new int[1];
            seedInBit.CopyTo(valueInDecimal, 0);
            BitArray Seed = new BitArray(new int[] { valueInDecimal[0] });
            RGBPixel[,] Imageenc = new RGBPixel[ImageOperations.GetHeight(ImageMatrix), ImageOperations.GetWidth(ImageMatrix)];
            byte valueInByte = new byte();
            byte[] valueAftermodification = new byte[1];
            BitArray valueInBits;
            for (int i = 0; i < ImageOperations.GetHeight(ImageMatrix); i++)
            {
                for (int j = 0; j < ImageOperations.GetWidth(ImageMatrix); j++)
                {
                    valueInByte = ImageMatrix[i, j].red;
                    valueInBits = new BitArray(new byte[] { valueInByte });
                    BitArray SeedAfterShifting = ImageOperations.xorKEy(ref Seed, length, Tap);
                    ImageOperations.xorr(SeedAfterShifting, valueInBits).CopyTo(valueAftermodification, 0);
                    Imageenc[i, j].red = valueAftermodification[0];

                    valueInByte = ImageMatrix[i, j].green;
                    valueInBits = new BitArray(new byte[] { valueInByte });
                    SeedAfterShifting = ImageOperations.xorKEy(ref Seed, length, Tap);
                    ImageOperations.xorr(SeedAfterShifting, valueInBits).CopyTo(valueAftermodification, 0);
                    Imageenc[i, j].green = valueAftermodification[0];

                    valueInByte = ImageMatrix[i, j].blue;
                    valueInBits = new BitArray(new byte[] { valueInByte });
                    SeedAfterShifting = ImageOperations.xorKEy(ref Seed, length, Tap);
                    ImageOperations.xorr(SeedAfterShifting, valueInBits).CopyTo(valueAftermodification, 0);
                    Imageenc[i, j].blue = valueAftermodification[0];
                }
            }
            ImageMatrix = Imageenc;
        }
        public static BitArray xorKEy(ref BitArray arr,int mostpos,int tap)
        {
            int []tempv=new int [1];
            arr.CopyTo(tempv,0);
            BitArray ret = new BitArray(8);
            for (int i = 7; i >=0; i--)
            {
                if (arr[mostpos] == arr[tap]) ret[i] = false;
                else ret[i] = true;
                tempv[0] = tempv[0] *2;
                if(ret[i]==false)
                    arr=new BitArray (new int []{tempv[0]});
                else
                {
                    tempv[0] =tempv[0]+ 1;
                    arr = new BitArray(new int[] { tempv[0] });
                }
            }
            return ret;
        }
        public static BitArray xorr(BitArray arr1, BitArray arr2)
        {
            BitArray b = new BitArray(8);
            for (int i = 0; i < 8;i++ )
            {
                if(arr1[i]==arr2[i])
                    b[i] = false;
                else
                    b[i] = true;
            }
                return b;
        }
//########################################################################################################################################################
        public static void tree(ref RGBPixel[,] ImageMatrix, ref node root_red, ref node root_green, ref node root_blue)
        {
            List<node> fred = new List<node>();
            List<node> fgreen = new List<node>();
            List<node> fblue = new List<node>();
            ImageOperations.make_freq(ImageMatrix, ref fred, ref fgreen, ref fblue);
            using (StreamWriter writetext = new StreamWriter("red.txt"))
            {
                for (int i = 0; i < fred.Count; i++)
                    writetext.WriteLine(fred[i].value + "  " + fred[i].freq);
            }
            using (StreamWriter writetext = new StreamWriter("green.txt"))
            {
                for (int i = 0; i < fgreen.Count; i++)
                    writetext.WriteLine(fgreen[i].value + "  " + fgreen[i].freq);
            }
            using (StreamWriter writetext = new StreamWriter("blue.txt"))
            {
                for (int i = 0; i < fblue.Count; i++)
                    writetext.WriteLine(fblue[i].value + "  " + fblue[i].freq);
            }
            root_red = ImageOperations.tree_color(fred);
            root_green = ImageOperations.tree_color(fgreen);
            root_blue = ImageOperations.tree_color(fblue);
        }
        public static void make_freq(RGBPixel[,] ImageMatrix, ref List<node> fred, ref List<node> fgreen, ref List<node> fblue)
        {   for (int i = 0; i < ImageMatrix.GetLength(0); i++)
                for (int g = 0; g < ImageMatrix.GetLength(1); g++)
                {   bool k = false;
                    int x = (int)ImageMatrix[i, g].red;
                    for (int y = 0; y < fred.Count; y++)
                    {   int ww = (int)fred[y].value;
                        if (ww == x)
                        {   fred[y].freq++;
                            k = true;
                            break;
                        }
                    }
                    if (k == false)
                    {    node temp = new node(x, 1);
                        fred.Add(temp);
                    }
                    k = false;
                    x = (int)ImageMatrix[i, g].green;
                    for (int y = 0; y < fgreen.Count; y++)
                    {   if (fgreen[y].value == x)
                        {   fgreen[y].freq++;
                            k = true;
                            break;
                        }
                    }
                    if (k == false)
                    {   node temp = new node(x, 1);
                        fgreen.Add(temp);
                    }
                    k = false;
                    x = (int)ImageMatrix[i, g].blue;
                    for (int y = 0; y < fblue.Count; y++)
                    {
                        if (fblue[y].value == x)
                        {
                            fblue[y].freq++;
                            k = true;
                            break;
                        }
                    }
                    if (k == false)
                    {
                        node temp = new node(x, 1);
                        fblue.Add(temp);
                    }
                }
        }
        public static node tree_color(List<node> fcolor)
        {
            PriorityQueue pq = new PriorityQueue();
            for (int y = 0; y < fcolor.Count; y++)
                pq.Enqueue(fcolor[y]);
            while (pq.Count() != 1)
            {
                node xx1 = pq.Dequeue();
                node xx2 = pq.Dequeue();
                node sum = new node(-1, xx1.freq + xx2.freq);
                sum.right = xx1;
                sum.left = xx2;
                pq.Enqueue(sum);
            }
            node root = pq.Dequeue();
            return root;
        }
        public static void print1(node root, node c, StreamWriter writetext)
        {
            if (root.value != -1)
            {
                string s = "";
                fun(c, root.value, s);
                writetext.WriteLine(root.value + "   " + root.freq + "     " + lll);
                sumtion += lll.Length * root.freq;
            }
            if (root.left != null)
            {
                node cur = root.left;
                print1(cur, c, writetext);
            }
            if (root.right != null)
            {
                node cur1 = root.right;
                print1(cur1, c, writetext);
            }
        }
        public static int ret_sum()
        {
            int te = sumtion / 8;
            sumtion = 0;
            return te;
        }
        public static void make_list(node root, node c, StreamWriter writetext)
        {
            if (root.value != -1)
            {
                string s = "";
                fun(c, root.value, s);
                writetext.WriteLine(root.value);
                writetext.WriteLine(lll);
            }
            if (root.left != null)
            {
                node cur = root.left;
                make_list(cur, c, writetext);
            }
            if (root.right != null)
            {
                node cur1 = root.right;
                make_list(cur1, c, writetext);
            }
        }
        static void fun(node r, int v, string s)
        {
            if (r.value == v)
            {
                lll = s;
                return;
            }
            if (r.left != null)
            {
                string h = s + '0';
                fun(r.left, v, h);
            }
            if (r.right != null)
            {
                string h1 = s + '1';
                fun(r.right, v, h1);
            }
        }
//########################################################################################################################################################
        public static void comprassion(RGBPixel[,] ImageMatrix, StreamWriter writetext1)
        {
            using (StreamWriter writetext = new StreamWriter("width_high.txt"))
            {   writetext.WriteLine(ImageMatrix.GetLength(0));
                writetext.WriteLine(ImageMatrix.GetLength(1));
            }
            List<int> red_v = new List<int>();
            List<int> green_v = new List<int>();
            List<int> blue_v = new List<int>();
            List<string> red_code = new List<string>();
            List<string> green_code = new List<string>();
            List<string> blue_code = new List<string>();
            string line;
            //*************************************************
            StreamReader file = new StreamReader("code_red.txt");
            while ((line = file.ReadLine()) != null)
            {   red_v.Add(int.Parse(line));
                line = file.ReadLine();
                red_code.Add(line);
            }
            file.Close();
            //*************************************************
            StreamReader file1 = new StreamReader("code_green.txt");
            while ((line = file1.ReadLine()) != null)
            {   green_v.Add(int.Parse(line));
                line = file1.ReadLine();
                green_code.Add(line);
            }
            file1.Close();
            //*************************************************
            StreamReader file2 = new StreamReader("code_blue.txt");
            while ((line = file2.ReadLine()) != null)
            {
                blue_v.Add(int.Parse(line));
                line = file2.ReadLine();
                blue_code.Add(line);
            }
            file2.Close();
            //*************************************************
            int idex = 0;
            BitArray b1 = new BitArray(1000000000);
            for (int i = 0; i < ImageMatrix.GetLength(0); i++)
                for (int f = 0; f < ImageMatrix.GetLength(1); f++)
                {
                    byte b = ImageMatrix[i, f].red;
                    for (int o = 0; o < red_v.Count; o++)
                    {
                        if (red_v[o] == (int)b)
                        {
                            int y = 0;
                            string temp = red_code[o];
                            while (y < temp.Length)
                            {
                                if (temp[y] == '0') b1[idex] = false;
                                else b1[idex] = true;
                                y++;
                                idex++;
                            }
                            break;
                        }
                    }
                    //*************************************************
                    b = ImageMatrix[i, f].green;
                    for (int o = 0; o < green_v.Count; o++)
                    {   if (green_v[o] == (int)b)
                        {   int y = 0;
                            string temp = green_code[o];
                            while (y < temp.Length)
                            {
                                if (temp[y] == '0') b1[idex] = false;
                                else b1[idex] = true;
                                y++;
                                idex++;
                            }
                            break;
                        }
                    }
                    //*************************************************
                    b = ImageMatrix[i, f].blue;
                    for (int o = 0; o < blue_v.Count; o++)
                    {   if (blue_v[o] == (int)b)
                        {   int y = 0;
                            string temp = blue_code[o];
                            while (y < temp.Length)
                            {
                                if (temp[y] == '0') b1[idex] = false;
                                else b1[idex] = true;
                                y++;
                                idex++;
                            }
                            break;
                        }
                    } 
                }
            //*************************************************
            int k = 0;
            int g = 0;
            BitArray ch = new BitArray(8);
            while (k < idex)
            {
                while (g < 8 && k < idex)
                {
                    ch[g] = b1[k];
                    g++;
                    k++;
                }
                g = 0;
                byte[] b = new byte[1];
                ch.CopyTo(b, 0);
                char v = (char)b[0];
                writetext1.Write(v);
                ch = new BitArray(8);
            }
        }
//########################################################################################################################################################
        public static void decomprassion1(ref RGBPixel[,] ImageMatrix, node r_red, node r_green, node r_blue)
        {
            char ch;
            StreamReader reader;
            FileInfo ff = new FileInfo("comprass.txt");
            int gg = (int)ff.Length;
            BitArray b2 = new BitArray(gg * 8);
            int index1 = 0;
            int index = -1;
            reader = new StreamReader("comprass.txt");
            while (!reader.EndOfStream)
            {
                ch = (char)reader.Read();
                byte bn = (byte)ch;
                BitArray bc = new BitArray(new byte[] { bn });
                for (int ii = 0; ii < 8; ii++)
                {
                    b2[index1] = bc[ii];
                    index1++;
                }
            }
            reader.Close();
            for (int i = 0; i < ImageMatrix.GetLength(0); i++)
                for (int f = 0; f < ImageMatrix.GetLength(1); f++)
                {
                    ImageMatrix[i, f].red = decomprss1(r_red, b2, ref index);
                    ImageMatrix[i, f].green = decomprss1(r_green, b2, ref index);
                    ImageMatrix[i, f].blue = decomprss1(r_blue, b2, ref index);
                }
        }
        public static byte decomprss1(node crrunt, BitArray arr, ref int m)
        {
            byte ret = 0;
            while (m != arr.Count)
            {
                m++;
                bool bit = arr[m];
                if (bit == true) crrunt = crrunt.right;
                else crrunt = crrunt.left;
                if (crrunt.left == null && crrunt.right == null)
                {
                    ret = (byte)crrunt.value;
                    break;
                }
            }
            return ret;
        }
//########################################################################################################################################################
        public static void decomprassion2(ref RGBPixel[,] ImageMatrix,
            List<string> r_code, List<string> g_code, List<string> b_code, List<int> r_v, List<int> g_v, List<int> b_v)
        {
            char ch;
            StreamReader reader;
            FileInfo ff = new FileInfo("comprass.txt");
            int gg = (int)ff.Length;
            BitArray b2 = new BitArray(gg * 8);
            int index1 = 0;
            int index = -1;
            reader = new StreamReader("comprass.txt");
            while (!reader.EndOfStream)
            {
                ch = (char)reader.Read();
                byte bn = (byte)ch;
                BitArray bc = new BitArray(new byte[] { bn });
                for (int ii = 0; ii < 8; ii++)
                {
                    b2[index1] = bc[ii];
                    index1++;
                }
            }
            reader.Close();
            for (int i = 0; i < ImageMatrix.GetLength(0); i++)
                for (int f = 0; f < ImageMatrix.GetLength(1); f++)
                {
                    ImageMatrix[i, f].red = decomprss2(r_code, r_v, b2, ref index);
                    ImageMatrix[i, f].green = decomprss2(g_code, g_v, b2, ref index);
                    ImageMatrix[i, f].blue = decomprss2(b_code, b_v, b2, ref index);
                }
        }
        public static byte decomprss2(List<string> code, List<int> value, BitArray arr, ref int m)
        {
            string temp = "";
            byte ret = 0;
            bool t = false;
            while (m != arr.Count)
            {
                m++;
                if (arr[m] == true) temp = temp + '1';
                else temp = temp + '0';
                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i] == temp)
                    {
                        ret = (byte)value[i];
                        t = true;
                        break;
                    }
                }
                if (t == true)
                {
                    break;
                }
            }
            return ret;
        }
    }
}
//########################################################################################################################################################
    public class node 
        {
            public int value;
            public int freq;
            public node left;
            public node right;
            public node(int v, int fr)
            {
                this.value = v;
                this.freq = fr;
                this.left = null;
                this.right = null;
            }
            public node()
            {}
         }
//########################################################################################################################################################
    public class PriorityQueue 
        {
            private List<node> data;
            public PriorityQueue()
            {
                this.data = new List<node>();
            }

            public node Dequeue()
            {
                node t= data[0];
                data.RemoveAt(0);
                return t; 
            }
           
            public int Count()
            {
                return data.Count;
            }
            public void Enqueue(node item)
            {
                if (data.Count == 0)
                    data.Add(item);
                else if (item.freq >= data[data.Count - 1].freq)
                    data.Add(item);

                else if (item.freq <= data[0].freq)
                {
                    node t = data[data.Count - 1];
                    for (int u = data.Count - 1; u > 0; u--)
                        data[u] = data[u - 1];
                    data[0] = item;
                    data.Add(t);
                }
                else
                {
                    int x = enq(0, data.Count - 1, item);
                    node t = data[data.Count - 1];
                    for (int u = data.Count - 1; u > x; u--)
                        data[u] = data[u - 1];
                    data[x] = item;
                    data.Add(t);
                }
            }
            public int enq(int low, int high, node i)
            {
                int mid = low + ((high - low) / 2);
                if (low > high)
                    return low;
                if (i.freq <= data[mid].freq)
                    return enq(low, mid - 1, i);
                else
                    return enq(mid + 1, high, i);
            }
        }
//########################################################################################################################################################
    

