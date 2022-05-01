using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StarMapCreator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private const int TVIF_STATE = 0x8;
        private const int TVIS_STATEIMAGEMASK = 0xF000;
        private const int TV_FIRST = 0x1100;
        private const int TVM_SETITEM = TV_FIRST + 63;
        private Color targetColor = Color.White;
        private bool 启用新画板 = false;
        private static void HideCheckBox(Control treeView, CheckBox node)
        {
            TVITEM tVITEM = new TVITEM();
            tVITEM.hItem = node.Handle;
            tVITEM.mask = TVIF_STATE;
            tVITEM.stateMask = TVIS_STATEIMAGEMASK;
            tVITEM.state = 0;
            SendMessage(treeView.Handle, TVM_SETITEM, IntPtr.Zero, ref tVITEM);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Auto)]
        private struct TVITEM
        {
            public int mask;
            public IntPtr hItem;
            public int state;
            public int stateMask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpszText;
            public int cchTextMax;
            public int iImage;
            public int iSelectedImage; public int cChildren; public IntPtr lParam;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, ref TVITEM lParam);


        List<CheckBox> checkBoxlist = new List<CheckBox>();
        public const int RoomSize = 8;
        Panel[,] checks = new Panel[RoomSize, RoomSize];
        class oop
        {
            public Panel p;
            public Color 颜色;
            public Color 新颜色;
        }
        List<oop> 撤回列表 = new List<oop>();
        private bool isDown = false;
        private Panel lastPanel;
        private void initPan()
        {
            //这里的逻辑还需要重构 读取PrintForm.room.colors 来动态生成画板即可
            var j = 0; var i = 0;
            var c = new Panel();
            c.SetBounds(label2.Location.X + j * 22, label2.Location.Y + 40 + i * 22, 20, 20);
            c.Parent = this;
            checks[i, j] = c;
            c.BackColor = Color.White;
            lastPanel = c;
            c.BorderStyle = BorderStyle.Fixed3D;
            c.MouseClick += new MouseEventHandler((object sender, MouseEventArgs e) =>
            {
                var d = sender as Panel;
                targetColor = d.BackColor;
                pf.DrawColorId = 0;
                lastPanel.BorderStyle = BorderStyle.None;
                Debug.WriteLine("当前目标颜色-白色");
                d.BorderStyle = BorderStyle.Fixed3D;
                lastPanel = d;
            });
            j = 1; i = 0;
            c = new Panel();
            c.SetBounds(label2.Location.X + j * 22 + 10, label2.Location.Y + 40 + i * 22, 20, 20);
            c.Parent = this;
            checks[i, j] = c;
            c.BackColor = Color.Black;
            c.MouseClick += new MouseEventHandler((object sender, MouseEventArgs e) =>
            {
                var d = sender as Panel;
                targetColor = d.BackColor;
                pf.DrawColorId = 1;
                Debug.WriteLine("当前目标颜色-黑色");
                lastPanel.BorderStyle = BorderStyle.None;
                d.BorderStyle = BorderStyle.Fixed3D;
                lastPanel = d;
            });

            //c.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            // c.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseUp);
        }
        public static PrintForm pf;
        private void Form1_Load(object sender, EventArgs e)
        {
            
            //初始化画板
            pf = new PrintForm();
            pf.father = this;
            pf.TopLevel = false;
            pf.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            pf.Dock = DockStyle.Fill;
            pf.Parent = this.panel1;
            pf.Show();
            //初始化色盘
            initPan();
            //
            //return;
            int k = 0;
            for (var i = 0; i < RoomSize; i++)
            {
                for (var j = 0; j < RoomSize; j++)
                {
                    var c = new Panel();
                    c.SetBounds(40 + j * 22, 40 + i * 22, 20, 20);
                    c.Parent = this;
                    checks[i, j] = c;
                    c.BackColor = Color.White;
                    c.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
                    c.MouseEnter += new System.EventHandler(this.Form1_MouseEnter);
                    c.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseUp);

                    //c.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseMove);
                    //HideCheckBox(this, c);
                }
            }
            int a = RoomSize / 2;
            int b = a - 1;
            int cc = RoomSize - 1;
            for (var i = 0; i < RoomSize; i++)
            {
                if (i != a && i != b)
                    checks[0, i].BackColor = Color.Black;
                //checks[0, i].Checked = true;
            }
            for (var i = 0; i < RoomSize; i++)
            {
                if (i != a && i != b)
                    checks[cc, i].BackColor = Color.Black;
            }
            for (var i = 0; i < RoomSize; i++)
            {
                if (i != a && i != b)
                    checks[i, 0].BackColor = Color.Black;
            }
            for (var i = 0; i < RoomSize; i++)
            {
                if (i != a && i != b)
                    checks[i, cc].BackColor = Color.Black;
            }
            this.checkBox2.Checked = true;
        }
        private void clear()
        {
            if (启用新画板)
            {
                pf.clear();
                return;
            }
            int a = RoomSize / 2;
            int b = a - 1;
            int cc = RoomSize - 1;

            for (var i = 0; i < RoomSize; i++)
            {
                for (var j = 0; j < RoomSize; j++)
                {
                    var c = checks[i, j];
                    c.BackColor = Color.White;
                }
            }
            for (var i = 0; i < RoomSize; i++)
            {
                if (i != a && i != b)
                    checks[0, i].BackColor = Color.Black;
                //checks[0, i].Checked = true;
            }
            for (var i = 0; i < RoomSize; i++)
            {
                if (i != a && i != b)
                    checks[cc, i].BackColor = Color.Black;
            }
            for (var i = 0; i < RoomSize; i++)
            {
                if (i != a && i != b)
                    checks[i, 0].BackColor = Color.Black;
            }
            for (var i = 0; i < RoomSize; i++)
            {
                if (i != a && i != b)
                    checks[i, cc].BackColor = Color.Black;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            int k = 0;
            string d = textBox1.Text;
            for (var i = 0; i < RoomSize; i++)
            {
                for (var j = 0; j < RoomSize; j++)
                {
                    if (!启用新画板)
                    {
                        var c = checks[i, j];
                        sb.Append("SMCRooms_bits[" + (k++) + "][" + d + "]=" + (c.BackColor == Color.White ? 0 : 1) + ";");
                    }
                    else
                    {
                        var c = pf.rooms[i, j];
                        sb.Append("SMCRooms_bits[" + (k++) + "][" + d + "]=" + PrintForm.GetIdByColor(c.color) + ";");
                    }
                }
                sb.Append("\n");
            }
            richTextBox1.Text = "";
            if (checkBox1.Checked)
            {
                richTextBox1.AppendText("private function SMC_InitRoomType_" + d + "(){\n");
                richTextBox1.AppendText(sb.ToString());
                richTextBox1.AppendText("}\n");
            }
            else
            {
                richTextBox1.AppendText(sb.ToString());
            }

        }
        const int size = RoomSize;
        const int size2 = size * size;
        /// <summary>
        /// 输出旋转矩阵索引代码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            this.richTextBox1.Text = "";
            StringBuilder sb = new StringBuilder();
            int[] line = new int[size2];
            int[] outline2 = new int[size2];
            int[] outline = new int[size2];
            sb.Append("private function SMC_InitMatrixRotation(){\n");
            for (var i = 0; i < size2; i++)
            {
                line[i] = i;
                sb.Append("SMC_MatrixRotationKey[");
                sb.Append(0 + i);
                sb.Append("] = ");
                sb.Append(line[i] + ";");
                if (i % size == (size - 1))
                {
                    sb.Append("\n");
                }
            }
            sb.Append("\n");
            var hfsize = size / (size - 1);
            //右旋转90°
            for (var i = 0; i < size2; i++)
            {
                outline[i] = line[i % size * size + i / size];
            }
            for (var i = 0; i < size; i++)
            {
                var aaa = i * RoomSize;
                var bbb = aaa + RoomSize;
                for (var j = aaa; j < bbb; j++)
                {
                    for (var k = j + 1; k < bbb; k++)
                    {
                        var t = outline[j];
                        var temp = outline[k];
                        if (t < temp)
                        {
                            outline[k] = t;
                            outline[j] = temp;
                        }
                    }
                }
            }
            for (var i = 0; i < size2; i++)
            {
                sb.Append("SMC_MatrixRotationKey[");
                sb.Append(3000 + i);
                sb.Append("] = ");
                sb.Append(outline[i] + ";");
                if (i % size == (size - 1))
                {
                    sb.Append("\n");
                }
                line[i] = outline[i];
            }
            sb.Append("\n");
            for (var i = 0; i < size2; i++)
            {
                outline[i] = line[i % size * size + i / size];
            }
            for (var i = 0; i < size; i++)
            {
                var aaa = i * RoomSize;
                var bbb = aaa + RoomSize;
                for (var j = aaa; j < bbb; j++)
                {
                    for (var k = j + 1; k < bbb; k++)
                    {
                        var t = outline[j];
                        var temp = outline[k];
                        if (t < temp)
                        {
                            outline[k] = t;
                            outline[j] = temp;
                        }
                    }
                }
            }
            for (var i = 0; i < size2; i++)
            {
                sb.Append("SMC_MatrixRotationKey[");
                sb.Append(2000 + i);
                sb.Append("] = ");
                sb.Append(outline[i] + ";");
                if (i % size == (size - 1))
                {
                    sb.Append("\n");
                }
                line[i] = outline[i];
            }
            sb.Append("\n");
            for (var i = 0; i < size2; i++)
            {
                outline[i] = line[i % size * size + i / size];
            }
            for (var i = 0; i < size; i++)
            {
                var aaa = i * RoomSize;
                var bbb = aaa + RoomSize;
                for (var j = aaa; j < bbb; j++)
                {
                    for (var k = j + 1; k < bbb; k++)
                    {
                        var t = outline[j];
                        var temp = outline[k];
                        if (t < temp)
                        {
                            outline[k] = t;
                            outline[j] = temp;
                        }
                    }
                }
            }
            for (var i = 0; i < size2; i++)
            {
                sb.Append("SMC_MatrixRotationKey[");
                sb.Append(1000 + i);
                sb.Append("] = ");
                sb.Append(outline[i] + ";");
                if (i % size == (size - 1))
                {
                    sb.Append("\n");
                }
                line[i] = outline[i];
            }
            sb.Append("}\n");
            this.richTextBox1.Text = sb.ToString();
        }

        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            // Debug.WriteLine((isDown?"按下":"抬起") + sender.GetType().Name);
            if (isDown)
            {
                if (sender.GetType().Name == "Panel")
                {
                    // Debug.WriteLine(sender.GetType().Name);
                    Panel p = sender as Panel;
                    if (p.BackColor != targetColor)
                    {

                        var oop = new oop();
                        oop.p = p;
                        oop.颜色 = p.BackColor;
                        p.BackColor = targetColor;
                        撤回列表.Add(oop);
                    }
                }
            }
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            //Debug.WriteLine("按下");
            if (e.Button == MouseButtons.Right)
            {
                isDown = !isDown;
                if (isDown)
                {
                    撤回列表.Clear();
                }
                if (sender.GetType().Name == "Panel")
                {
                    // Debug.WriteLine(sender.GetType().Name);
                    Panel p = sender as Panel;
                    if (p.BackColor != targetColor)
                    {

                        var oop = new oop();
                        oop.p = p;
                        oop.颜色 = p.BackColor;
                        p.BackColor = targetColor;
                        撤回列表.Add(oop);
                    }
                }
            }

        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            //Debug.WriteLine("抬起");
            //isDown = false;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (false || isDown)
            {
                if (sender.GetType().Name == "Panel")
                {
                    Debug.WriteLine(sender.GetType().Name);
                    Panel p = sender as Panel;
                    p.BackColor = targetColor;
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (启用新画板)
            {
                pf.撤回一步();
                return;
            }
            foreach (var p in 撤回列表)
            {
                p.p.BackColor = p.颜色;
            }
            撤回列表.Clear();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            clear();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox2.Checked)
            {
                启用新画板 = true;
                this.panel1.Visible = true;
            }
            else
            {
                启用新画板 = false;
                this.panel1.Visible = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (启用新画板)
                pf.恢复一步();
        }
        private string SelectPath(bool issave)
        {
            string path = string.Empty;
            if (!issave)
            {

                OpenFileDialog openFileDialog = new OpenFileDialog()
                {
                    Filter = "Files (*.json)|*.json"//如果需要筛选txt文件（"Files (*.txt)|*.txt"）
                };

                //var result = openFileDialog.ShowDialog();
                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    path = openFileDialog.FileName;
                }
                else
                {
                    path = null;
                }
            }
            else
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.Description = "请选择文件路径";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    path = dialog.SelectedPath + @"\";
                }
                else
                {
                    path = null;
                }
            }

            return path;
        }
        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var path = FilePathHelper.SelectFile();

            if (!string.IsNullOrEmpty(path))
            {
                var str = File.ReadAllText(path);

                Dictionary<string, object> d = new Dictionary<string, object>();
                JsonConvert.PopulateObject(str, d);
                Debug.WriteLine(d["list"].GetType());
                JArray ary = d["list"] as JArray;


                PrintForm.在矩形渲染时 = true;
                for (var i = 0; i < RoomSize; i++)
                {
                    for (var j = 0; j < RoomSize; j++)
                    {
                        var c = pf.rooms[i, j];
                        var id = (int)ary[i * RoomSize + j];
                        c.resetColor(id);
                    }
                }
                PrintForm.在矩形渲染时 = false;
                PrintForm.更新撤回列表();
            }
        }

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int[] outAry = new int[RoomSize * RoomSize];
            for (var i = 0; i < RoomSize; i++)
            {
                for (var j = 0; j < RoomSize; j++)
                {
                    var c = pf.rooms[i, j];
                    outAry[i * RoomSize + j] = PrintForm.GetIdByColor(c.color);
                }
            }

            Dictionary<string, object> d = new Dictionary<string, object>();
            d.Add("list", outAry);
            string json = JsonConvert.SerializeObject(d);
            string result = json;  //输入文本
            var path = FilePathHelper.SaveFilePathName("save.json", "Files (*.json)|*.json");
            if (!string.IsNullOrEmpty(path))
            {
                StreamWriter sw = File.CreateText(path); //保存到指定路径
                sw.Write(result);
                sw.Flush();
                sw.Close();
            }
        }
        /// <summary>
        /// 右旋90°
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            if(启用新画板)
            {
                StringBuilder sb = new StringBuilder();
                int[] line = new int[size2];
                int[] index = new int[size2];
                int[] outline = new int[size2];
                var hfsize = size / (size - 1)+1;
                //初始化矩阵
                for (var i = 0; i < RoomSize; i++)
                {

                    for (var j = 0; j < RoomSize; j++)
                    {
                        var c = pf.rooms[i, j];
                        index[i * RoomSize + j] = i * RoomSize + j;
                        line[i * RoomSize + j] = PrintForm.GetIdByColor(c.color);
                        //sb.Append(line[i * RoomSize + j]);
                    }
                    //sb.Append("\n");
                }
                //sb.Append("右旋转90°\n");
                # region 右旋转90°矩阵旋转
                    for (var i = 0; i < size2; i++)
                    {
                        outline[i] = index[i % size * size + i / size];
                    }
                    for (var i = 0; i < size; i++)
                    {
                        for (var j = 0; j < size; j++)
                        {
                            var temp = outline[i * size + j];
                            outline[i * size + j] = outline[i * size + (size - 1) - j];
                            outline[i * size + (size - 1) - j] = temp;
                        }
                    }
                    PrintForm.启用矩形渲染 = true; //该标记用于撤销
                    for (var i = 0; i < size; i++)
                    {
                        var aaa = i * RoomSize;
                        var bbb = aaa + RoomSize;
                        for (var j = aaa; j < bbb; j++)
                        {
                            for (var k = j+1; k < bbb; k++)
                            {
                                var t = outline[j];
                                var temp = outline[k];
                                if (t < temp)
                                {
                                    outline[k] = t;
                                    outline[j] = temp;
                                }
                            }
                        }
                        //渲染部分
                        for (var j = 0; j < size; j++)
                        {
                            var c = pf.rooms[i, j];
                            c.resetColor(line[outline[i * size + j]]);
                            //sb.Append(outline[i * size + j] + "  ");
                        }
                        //sb.Append("\n");
                    }
                    //PrintForm.This.father.richTextBox1.Text = "";
                    //PrintForm.This.father.richTextBox1.AppendText(sb.ToString());
                    PrintForm.启用矩形渲染 = false;
                    PrintForm.更新撤回列表();
                #endregion
            }
        }
    }


    public class FilePathHelper
    {
        /// <summary>
        /// 选择保存文件的名称以及路径  取消返回 空"";
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="filter"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string SaveFilePathName(string fileName = null, string filter = null, string title = null)
        {
            string path = "";
            System.Windows.Forms.SaveFileDialog fbd = new System.Windows.Forms.SaveFileDialog();
            if (!string.IsNullOrEmpty(fileName))
            {
                fbd.FileName = fileName;
            }
            if (!string.IsNullOrEmpty(filter))
            {
                fbd.Filter = filter;// "Excel|*.xls;*.xlsx;";
            }
            if (!string.IsNullOrEmpty(title))
            {
                fbd.Title = title;// "保存为";
            }
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = fbd.FileName;
            }
            return path;
        }
        /// <summary>
        /// 选择一个文件
        /// </summary>
        /// <param name="filter">如果需要筛选txt文件（"Files (*.txt)|*.txt"）</param>
        /// <returns></returns>
        public static string SelectFile(string filter = null)
        {
            string path = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Files (*.json)|*.json"//如果需要筛选txt文件（"Files (*.txt)|*.txt"）
            };

            //var result = openFileDialog.ShowDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = openFileDialog.FileName;
            }
            else
            {
                path = null;
            }
            return path;
        }

        /// <summary>
        /// 选择一个路径
        /// </summary>
        /// <returns></returns>
        public static string SelectPath()
        {
            string path = string.Empty;
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = fbd.SelectedPath;
            }
            return path;
        }
    }


}
