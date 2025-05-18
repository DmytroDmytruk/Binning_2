using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;

namespace VolosIndiv
{
    public partial class Form1 : Form
    {
        List<float> x;
        List<float> y;
        List<int> OneCol;

        bool IsWorking = false;


        public Form1()
        {
            InitializeComponent();
            saveFileDialog1.DefaultExt = "*.txt";
            saveFileDialog1.Filter = "TXT Files|*.txt";
            dataGridView1.ColumnCount = 3;
        }


        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsWorking)
            {
                IsWorking = false;
                e.Cancel = true;
            }
        }

        private void зберегтиДаніToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridView dgv = new DataGridView();

            if (tabControl1.SelectedIndex == 0)
                dgv = dataGridView1;
            else if (tabControl1.SelectedIndex == 1)
                dgv = dataGridView2;
            else if (tabControl1.SelectedIndex == 2)
                dgv = dataGridView3;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (var sw = new StreamWriter(saveFileDialog1.FileName))
                {
                    for (int i = 0; i < dgv.Rows.Count; i++)
                    {
                        List<string> values = new List<string>();

                        for (int j = 0; j < dgv.Columns.Count; j++)
                        {
                            var cellValue = dgv.Rows[i].Cells[j].Value;
                            if (cellValue != null)
                                values.Add(cellValue.ToString());
                        }

                        sw.WriteLine(string.Join("\t", values));
                    }
                }
                MessageBox.Show("Дані збережено");
            }
        }

        private void бінуванняToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (x != null && y != null)
            {

                if (x.Count > 0 && y.Count > 0)
                    BinningTwoColumn();
                else if (OneCol.Count > 0)
                    BinningOneColumn();
            }
            else
            {
                MessageBox.Show("Даних немає!");
            }
        }

        private void зТекстівToolStripMenuItem_Click(object sender, EventArgs e)
        {
            x = new List<float>();
            y = new List<float>();


            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                Thread th = new Thread(() => GetDataFromFiles(folderBrowserDialog1.SelectedPath));
                th.Start();
            }
        }

        private void зФайлуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetDataFromFileTwo();
            label11.Text = "Дані завантажено!";
        }

        private void зФайлуДвіКолонкиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetDataFromFileOne();
            label11.Text = "Дані завантажено!";
        }


        private void GetDataFromFiles(string path)
        {
            var files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
            var l = files.Length;
            IsWorking = true;

            Invoke(new Action(() =>
            {
                menuStrip1.Enabled = false;
                progressBar1.Maximum = l;
                progressBar1.Value = 0;
            }));

            for (int i = 0; i < l; i++)
            {
                x.Add(0);
                y.Add(0);
            }

            int kilk = 0;

            Parallel.For(0, l, (i, loopState) =>
            {
                if (!IsWorking)
                    loopState.Break();

                kilk++;
                if (kilk % 50 == 0)
                {
                    Invoke(new Action(() =>
                    {
                        progressBar1.Value = kilk;
                        label11.Text = "оброблено текстів: {kilk}/{l}";
                    }));
                }

                using (var stream = new StreamReader(files[i], Encoding.Default, true))
                {
                    var localText = string.Join("", stream.ReadToEnd().Where(c => char.IsLetter(c) || c == ' '));
                    var wordsCount = localText.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < wordsCount.Length; j++)
                    {
                        wordsCount[j] = wordsCount[j].ToLower();
                    }
                    x[i] = wordsCount.Length;
                    y[i] = wordsCount.Distinct().Count();
                }
            });

            IsWorking = false;
            Invoke(new Action(() =>
            {
                menuStrip1.Enabled = true;
                label11.Text = "обробка текстів завершено! Дані отримано!";

                dataGridView1.Rows.Clear();
                for (int i = 0; i < files.Length; i++)
                    dataGridView1.Rows.Add(Path.GetFileNameWithoutExtension(files[i]), x[i].ToString(), y[i].ToString());
            }));
        }

        private void GetDataFromFileTwo()
        {
            x = new List<float>();
            y = new List<float>();

            int c1 = 0;
            int c2 = 1;

            int.TryParse(textBox3.Text, out c1);
            int.TryParse(textBox4.Text, out c2);

            if (c1 > c2)
            {
                var t = c1;
                c1 = c2;
                c2 = t;
            }

            if (openFileDialog1.ShowDialog() == DialogResult.OK && openFileDialog1.FileName.Length > 0)
            {
                string[] lines = File.ReadAllLines(openFileDialog1.FileName, Encoding.GetEncoding(1251));

                foreach (string line in lines)
                {
                    string[] res = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    float v1 = 0;
                    float v2 = 0;

                    if (res.Length >= c2)
                    {
                        string s1 = res[c1 - 1].Replace('.', ',');
                        string s2 = res[c2 - 1].Replace('.', ',');
                        float.TryParse(s1, out v1);
                        float.TryParse(s2, out v2);
                    }

                    x.Add(v1);
                    y.Add(v2);
                }
            }
        }

        private void GetDataFromFileOne()
        {
            x = new List<float>();
            y = new List<float>();
            OneCol = new List<int>();

            int c1 = 0;
            int c2 = 1;

            int.TryParse(textBox3.Text, out c1);
            int.TryParse(textBox4.Text, out c2);

            if (c1 > c2)
            {
                var t = c1;
                c1 = c2;
                c2 = t;
            }

            if (openFileDialog1.ShowDialog() == DialogResult.OK && openFileDialog1.FileName.Length > 0)
            {
                string[] lines = File.ReadAllLines(openFileDialog1.FileName, Encoding.GetEncoding(1251));

                foreach (string line in lines)
                {
                    string[] res = line.Split('\t');

                    int v1 = 0;

                    if (res.Length >= c1)
                    {
                        int.TryParse(res[c1 - 1], out v1);
                    }

                    OneCol.Add(v1);
                }
            }
        }


        private void BinningTwoColumn()
        {
            int M = 0;
            double basePow = 0;

            int.TryParse(textBox1.Text, out M);
            double.TryParse(textBox2.Text, out basePow);

            if (basePow <= 1)
            {
                MessageBox.Show("Основа степення повина бути більша 1!");
                return;
            }

            List<DataOut> dataOut = new List<DataOut>();

            if (radioButton1.Checked)
                dataOut = avg1(M);
            else if (radioButton2.Checked)
                dataOut = avg2(M);
            else if (radioButton3.Checked)
                dataOut = avg3(basePow);

            dataGridView2.Rows.Clear();
            for (int i = 0; i < dataOut.Count; i++)
            {
                dataGridView2.Rows.Add(i + 1, dataOut[i].LeftBorder+"-"+dataOut[i].RightBolder, dataOut[i].L, dataOut[i].V, dataOut[i].dV, dataOut[i].ItemCount);
            }
        }

        private void BinningOneColumn()
        {
            List<DataOutOneCol> dataOut = new List<DataOutOneCol>();

            var dict = OneCol.GroupBy(w => w).ToDictionary(g => g.Key, g => g.Count());
            var Values = dict.Values.ToList();
            double maxX = Values.Max();
            double minX = Values.Min();

            int M = 0;
            double basePow = 0;
            int maxSteps = 0;

            int.TryParse(textBox1.Text, out M);
            double.TryParse(textBox2.Text, out basePow);
            double step = (maxX - minX) / M;

            for (int i = 0; i < 99999; i++)
            {
                if (Math.Pow(basePow, i) > maxX)
                {
                    maxSteps = i;
                    break;
                }
            }

            dataGridView3.Rows.Clear();

            if (radioButton1.Checked || radioButton2.Checked)
            {
                for (int i = 0; i < M; i++)
                {
                    DataOutOneCol dataTemp = new DataOutOneCol();

                    dataTemp.LeftBorder = minX + i * step;
                    dataTemp.RightBorder = minX + (i + 1) * step;

                    var arrTemp = Values.Where(v => v >= dataTemp.LeftBorder && v <= dataTemp.RightBorder).ToList();
                    dataTemp.p = OneCol.Count != 0 ? arrTemp.Sum() / (double)OneCol.Count : 0;
                    dataTemp.ItemCount = arrTemp.Count;
                    dataOut.Add(dataTemp);
                }

            }
            else if (radioButton3.Checked)
            {
                for (int i = 0; i < maxSteps; i++)
                {
                    DataOutOneCol dataTemp = new DataOutOneCol();

                    dataTemp.LeftBorder = Math.Pow(basePow, i);
                    dataTemp.RightBorder = Math.Pow(basePow, i + 1);

                    var arrTemp = Values.Where(v => v >= dataTemp.LeftBorder && v < dataTemp.RightBorder).ToList();
                    dataTemp.p = OneCol.Count != 0 ? arrTemp.Sum() / (double)OneCol.Count : 0;
                    dataTemp.ItemCount = arrTemp.Count;
                    dataOut.Add(dataTemp);
                }
            }

            for (int i = 0; i < dataOut.Count; i++)
            {
                dataOut[i].P = dataOut.GetRange(i, dataOut.Count - i).Sum(x => x.p);
            }

            int j = 1;
            foreach (var item in dataOut)
            {
                dataGridView3.Rows.Add("{j}", "{Math.Round(item.LeftBorder, 2)} - {Math.Round(item.RightBorder, 2)}", "{item.p}", "{item.P}", "{item.ItemCount}");
                j++;
            }
        }


        private List<DataOut> avg1(int M)
        {
            float minX = x.Min();
            double step = (x.Max() - x.Min()) / (double)M;
            List<DataOut> dataOut = new List<DataOut>();

            for (int i = 0; i < M; i++)
            {
                double avgResL = 0;
                double avgQuadL = 0;
                double avgResV = 0;
                double avgQuadV = 0;
                double itemCount = 0;

                double leftBorder = minX + i * step;
                double rightBorder = minX + (i + 1) * step;

                for (int j = 0; j < x.Count; j++)
                {
                    if (x[j] >= leftBorder && x[j] < rightBorder)
                    {
                        avgResL += x[j];
                        avgQuadL += x[j] * x[j];
                        avgResV += y[j];
                        avgQuadV += y[j] * y[j];
                        itemCount++;
                    }
                }

                avgResL = itemCount != 0 ? avgResL / itemCount : 0;
                avgQuadL = itemCount != 0 ? avgQuadL / itemCount : 0;
                avgResV = itemCount != 0 ? avgResV / itemCount : 0;
                avgQuadV = itemCount != 0 ? avgQuadV / itemCount : 0;

                var dataTemp = new DataOut();
                dataTemp.ItemCount = itemCount;

                dataTemp.dV = Math.Round(Math.Sqrt(avgQuadV - avgResV * avgResV), 4);
                dataTemp.L = Math.Round(avgResL, 4);
                dataTemp.V = Math.Round(avgResV, 4);
                dataTemp.LeftBorder = Math.Round(leftBorder, 4);
                dataTemp.RightBolder = Math.Round(rightBorder, 4);

                dataOut.Add(dataTemp);
            }

            return dataOut;
        }

        private List<DataOut> avg2(int M)
        {
            List<DataOut> dataOut = new List<DataOut>();

            var xx = x.ToArray();
            var yy = y.ToArray();
            Array.Sort(xx, yy);
            x = xx.ToList();
            y = yy.ToList();

            int binSize = x.Count / M;

            for (int i = 0; i < M; i++)
            {
                int startIdx = i * binSize;
                int endIdx = (i == M - 1) ? x.Count : (i + 1) * binSize;

                double avgQuadL = 0;
                double avgQuadV = 0;
                double avgResL = 0;
                double avgResV = 0;
                double itemCount = 0;

                for (int j = startIdx; j < endIdx; j++)
                {
                    avgResL += x[j];
                    avgQuadL += x[j] * x[j];
                    avgResV += y[j];
                    avgQuadV += y[j] * y[j];
                    itemCount++;
                }

                avgResL = itemCount != 0 ? avgResL / itemCount : 0;
                avgQuadL = itemCount != 0 ? avgQuadL / itemCount : 0;
                avgResV = itemCount != 0 ? avgResV / itemCount : 0;
                avgQuadV = itemCount != 0 ? avgQuadV / itemCount : 0;

                var dataTemp = new DataOut();
                dataTemp.ItemCount = itemCount;
                dataTemp.dV = Math.Round(Math.Sqrt(avgQuadV - avgResV * avgResV), 4);
                dataTemp.L = Math.Round(avgResL, 4);
                dataTemp.V = Math.Round(avgResV, 4);
                dataTemp.LeftBorder = Math.Round(x[startIdx], 4);
                dataTemp.RightBolder = Math.Round(x[endIdx - 1], 4);

                dataOut.Add(dataTemp);
            }

            return dataOut;
        }

        private List<DataOut> avg3(double basePow)
        {
            double maxX = x.Max();
            List<DataOut> dataOut = new List<DataOut>();

            int maxSteps = 0;
            for (int i = 0; i < 99999; i++)
            {
                if (Math.Pow(basePow, i) > maxX)
                {
                    maxSteps = i;
                    break;
                }
            }

            for (int i = 0; i < maxSteps; i++)
            {
                double avgQuadL = 0;
                double avgQuadV = 0;
                double avgResL = 0;
                double avgResV = 0;
                double itemCount = 0;

                double leftBorder = Math.Pow(basePow, i);
                double rightBorder = Math.Pow(basePow, i + 1);

                for (int j = 0; j < x.Count; j++)
                {
                    if (x[j] >= leftBorder && x[j] < rightBorder)
                    {
                        avgResL += x[j];
                        avgQuadL += x[j] * x[j];
                        avgResV += y[j];
                        avgQuadV += y[j] * y[j];
                        itemCount++;
                    }
                }

                avgResL = itemCount != 0 ? avgResL / itemCount : 0;
                avgQuadL = itemCount != 0 ? avgQuadL / itemCount : 0;
                avgResV = itemCount != 0 ? avgResV / itemCount : 0;
                avgQuadV = itemCount != 0 ? avgQuadV / itemCount : 0;

                var dataTemp = new DataOut();
                dataTemp.ItemCount = itemCount;

                dataTemp.dV = Math.Round(Math.Sqrt(avgQuadV - avgResV * avgResV), 4);
                dataTemp.L = Math.Round(avgResL, 4);
                dataTemp.V = Math.Round(avgResV, 4);
                dataTemp.LeftBorder = Math.Round(leftBorder, 4);
                dataTemp.RightBolder = Math.Round(rightBorder, 4);

                dataOut.Add(dataTemp);
            }

            return dataOut;
        }

    }
}