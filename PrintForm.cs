using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StarMapCreator
{
    public partial class PrintForm : Form
    {
        /*
         todo list
        1.修改生成代码为vj代码
        2.增加保存/读取 数据为json
        3.
         
         */
        public PrintForm()
        {
            InitializeComponent();
            This = this;
        }
        #region 变量区
        /// <summary>
        /// 当前实例
        /// </summary>
        public static PrintForm This;
        public int DrawColorId = 0;
        /// <summary>
        /// 帧渲染计时器
        /// </summary>
        public Timer MainTimer = new Timer();
        /// <summary>
        /// 主窗体
        /// </summary>
        public Form1 father;
        /// <summary>
        /// 房间尺寸
        /// </summary>
        const int RoomSize = Form1.RoomSize;

        public room[,] rooms = new room[RoomSize, RoomSize];
        public static List<oop> 当前撤回列表 = new List<oop>();
        public static List<List<oop>> 历史撤回列表 = new List<List<oop>>();
        public static int 撤回列表指针 = 0;
        public static Point 鼠标按下位置 = new Point(0, 0);
        public static Point 鼠标松开位置 = new Point(0, 0);
        public static bool 有变更 = false;
        public static bool 在矩形渲染时 = false;
        public static Hashtable CIDhashtable = new Hashtable();
        public static void InitHT(ref Color[] cs) {
            for (var i = 0; i < cs.Length; i++)
            {
                CIDhashtable.Add(cs[i].GetHashCode(), i);
            }
        }
        /// <summary>
        /// 转换颜色 为 索引
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int GetIdByColor(Color c)
        {
            object o = CIDhashtable[c.GetHashCode()];
            if(o==null)
            { return 0; }
            int i = Convert.ToInt32(o);
            return i;
        }
        #endregion
        public static void 清空撤回列表()
        {
            当前撤回列表 = new List<oop>();
            历史撤回列表 = new List<List<oop>>();
            撤回列表指针 = 0;
        }

        #region 撤回相关
        /// <summary>
        /// 使撤回表向下走
        /// </summary>
        public static void 更新撤回列表()
        {
            if (有变更)
            {
                if (撤回列表指针 < 历史撤回列表.Count)
                {
                    历史撤回列表[撤回列表指针] = 当前撤回列表;
                    撤回列表指针 = 历史撤回列表.Count;
                }
                else
                {
                    历史撤回列表.Add(当前撤回列表);
                    撤回列表指针 = 撤回列表指针 + 1;
                }
                当前撤回列表 = new List<oop>();
                有变更 = false;
            }
        }
        public void 撤回一步()
        {
            if (撤回列表指针 > 0)
            {
                撤回列表指针 = 撤回列表指针 - 1;
                foreach (var o in 历史撤回列表[撤回列表指针])
                {
                    o.p.resetColor(o.颜色);
                }

            }
        }
        public void 恢复一步()
        {
            if (撤回列表指针 < 历史撤回列表.Count)
            {
                foreach (var o in 历史撤回列表[撤回列表指针])
                {
                    o.p.resetColor(o.新颜色);
                }
                撤回列表指针 = 撤回列表指针 + 1;
            }
        }
        #endregion

        public class room
        {
            /// <summary>
            /// 颜色列表
            /// </summary>
            static public Color[] colors = { Color.White, Color.Black };
            public float x;
            public float y;
            public float width;
            public float height;
            /// <summary>
            /// room笔刷
            /// </summary>
            public Pen pen = Pens.White;
            public Color color = Color.White;
            /// <summary>
            /// 重置颜色
            /// </summary>
            /// <param name="colorID"></param>
            public void resetColor(int colorID)
            {
                Color color = colors[colorID];
                if (color == this.color) { return; }
                oop o = new oop();
                o.颜色 = this.color;
                this.color = color;
                this.pen = new Pen(color);
                o.新颜色 = color;
                o.p = this;
                当前撤回列表.Add(o);
                if (鼠标左键按下)
                {
                    有变更 = true;
                }else if(在矩形渲染时)
                {
                    有变更 = true;
                }
            }
            public void resetColor(Color color)
            {
                //var cid = 0;
                //for (var i = 0; i < colors.Length; i++)
                //{
                //    if (color == colors[i])
                //    {
                //        cid = i;
                //    }
                //}
                resetColor(GetIdByColor(color));
            }
            public room(float x, float y, float width, float height)
            {
                this.x = x; this.y = y; this.width = width; this.height = height;
            }
        }
        /// <summary>
        /// 用于撤回的缓存对象
        /// </summary>
        public class oop
        {
            public room p;
            public Color 颜色;
            public Color 新颜色;
        }

        public void initRoomsDraw()
        {
            int k = 0;
            for (var i = 0; i < RoomSize; i++)
            {
                for (var j = 0; j < RoomSize; j++)
                {
                    var c = new room(40 + j * 22, 40 + i * 22, 20, 20);
                    rooms[i, j] = c;
                }
            }
            int a = RoomSize / 2;
            int b = a - 1;
            int cc = RoomSize - 1;
            for (var i = 0; i < RoomSize; i++)
            {
                if (i != a && i != b)
                    rooms[0, i].resetColor(GetIdByColor(Color.Black));
            }
            for (var i = 0; i < RoomSize; i++)
            {
                if (i != a && i != b)
                    rooms[cc, i].resetColor(GetIdByColor(Color.Black));
            }
            for (var i = 0; i < RoomSize; i++)
            {
                if (i != a && i != b)
                    rooms[i, 0].resetColor(GetIdByColor(Color.Black));
            }
            for (var i = 0; i < RoomSize; i++)
            {
                if (i != a && i != b)
                    rooms[i, cc].resetColor(GetIdByColor(Color.Black));
            }
            清空撤回列表();

        }

        public void DrawRoom(Graphics g, float x, float y, Pen pen)
        {
            g.ResetTransform();
            g.ScaleTransform((float)1, (float)1, MatrixOrder.Append); //scale
            g.TranslateTransform(x, y, MatrixOrder.Append); //pan

            if (pen.Color == Color.Black)
            {
                g.FillRectangle(new SolidBrush(Color.Gray), new Rectangle(0, 0, 20, 20));
                g.DrawRectangle(new Pen(pen.Color, 1), new Rectangle(0, 0, 20, 20));
                Font fo = new Font("微软雅黑", 10);
                Brush bru = new SolidBrush(pen.Color);
                g.TranslateTransform(5, 0, MatrixOrder.Append); //pan
                g.DrawString("1",fo,bru, PointF.Empty);
            }
            else
            {
                g.DrawRectangle(new Pen(pen.Color, 1), new Rectangle(0, 0, 20, 20));
            }
        }
        public void drawAllRoom(Graphics g)
        {
            for (var i = 0; i < RoomSize; i++)
            {
                for (var j = 0; j < RoomSize; j++)
                {
                    var c = rooms[i, j];
                    DrawRoom(g, c.x, c.y, c.pen);
                }
            }
        }

        public room getRoomUnMouse()
        {
            Point p2 = this.PointToClient(MousePosition);
            for (var i = 0; i < RoomSize; i++)
            {
                if (p2.Y > 40 + i * 22 && p2.Y < (40 + ((i + 1) * 22)))
                {
                    for (var j = 0; j < RoomSize; j++)
                    {
                        var c = rooms[i, j];
                        if (p2.X > c.x && p2.X < c.x + c.width)
                        {
                            return c;
                        }
                    }
                }
            }
            return null;
        }
        public void clear()
        {
            在矩形渲染时 = true;
            for (var i = 0; i < RoomSize; i++)
            {
                for (var j = 0; j < RoomSize; j++)
                {
                    rooms[i, j].resetColor(GetIdByColor(Color.White));
                }
            }
            更新撤回列表();
            int a = RoomSize / 2;
            int b = a - 1;
            int cc = RoomSize - 1;
            for (var i = 0; i < RoomSize; i++)
            {
                if (i != a && i != b)
                    rooms[0, i].resetColor(GetIdByColor(Color.Black));
            }
            for (var i = 0; i < RoomSize; i++)
            {
                if (i != a && i != b)
                    rooms[cc, i].resetColor(GetIdByColor(Color.Black));
            }
            for (var i = 0; i < RoomSize; i++)
            {
                if (i != a && i != b)
                    rooms[i, 0].resetColor(GetIdByColor(Color.Black));
            }
            for (var i = 0; i < RoomSize; i++)
            {
                if (i != a && i != b)
                    rooms[i, cc].resetColor(GetIdByColor(Color.Black));
            }
            在矩形渲染时 = false;
            更新撤回列表();
            //清空撤回列表();
        }
        private Rectangle currRect;//当前正在绘制的举行
        public static bool 启用矩形渲染 = true;
        public void PrintForm_Paint(object sender, PaintEventArgs e)
        {
            BufferedGraphicsContext currentContext = BufferedGraphicsManager.Current;
            BufferedGraphics myBuffer = currentContext.Allocate(e.Graphics, e.ClipRectangle);
            Graphics g = myBuffer.Graphics;
            g.Clear(Color.Gainsboro);
            g.SmoothingMode = SmoothingMode.HighSpeed;



            drawAllRoom(g);
            if (启用矩形渲染)
            {
                if (鼠标左键按下)
                {
                    鼠标松开位置 = PrintForm.This.PointToClient(MousePosition);
                    var startPoint = 鼠标按下位置;
                    var endPoint = 鼠标松开位置;
                    int realStartX = Math.Min(startPoint.X, endPoint.X);
                    int realStartY = Math.Min(startPoint.Y, endPoint.Y);
                    int realEndX = Math.Max(startPoint.X, endPoint.X);
                    int realEndY = Math.Max(startPoint.Y, endPoint.Y);
                    currRect = new Rectangle(realStartX, realStartY, realEndX - realStartX, realEndY - realStartY);
                    g.ResetTransform();
                    g.ScaleTransform((float)1, (float)1, MatrixOrder.Append); //scale
                    g.DrawRectangle(new Pen(Color.Gray, 1), currRect);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(153, room.colors[DrawColorId].R, room.colors[DrawColorId].G, room.colors[DrawColorId].B)), currRect);
                    for (var i = 0; i < RoomSize; i++)
                    {
                        for (var j = 0; j < RoomSize; j++)
                        {
                            var c = rooms[i, j];
                            if (PtInRect(new Point((int)(c.x + c.width / 2), (int)(c.y + c.height / 2)), currRect))
                            {
                                //c.resetColor(DrawColorId);
                                g.ResetTransform();
                                g.ScaleTransform((float)1, (float)1, MatrixOrder.Append); //scale
                                g.TranslateTransform(c.x, c.y, MatrixOrder.Append); //pan
                                g.FillRectangle(new SolidBrush(Color.FromArgb(200, room.colors[DrawColorId].R, room.colors[DrawColorId].G, room.colors[DrawColorId].B)), new Rectangle(0, 0, 20, 20));
                            }
                        }
                    }

                }
            }
            else
            {
                if (鼠标左键按下)
                {
                    //Debug.WriteLine("鼠标左键已经按下");
                    var c = getRoomUnMouse();
                    if (c != null)
                    {
                        c.resetColor(DrawColorId);
                    }
                }
            }
            myBuffer.Render(e.Graphics);
            g.Dispose();
            myBuffer.Dispose();

        }
        MessageFilter mf = new MessageFilter();
        public static bool 鼠标左键按下 = false;
        public void SetDraw()
        {
            if (启用矩形渲染)
            {
                在矩形渲染时 = true;
                var startPoint = 鼠标按下位置;
                var endPoint = 鼠标松开位置;
                int realStartX = Math.Min(startPoint.X, endPoint.X);
                int realStartY = Math.Min(startPoint.Y, endPoint.Y);
                int realEndX = Math.Max(startPoint.X, endPoint.X);
                int realEndY = Math.Max(startPoint.Y, endPoint.Y);
                currRect = new Rectangle(realStartX, realStartY, realEndX - realStartX, realEndY - realStartY);
                for (var i = 0; i < RoomSize; i++)
                {
                    for (var j = 0; j < RoomSize; j++)
                    {
                        var c = rooms[i, j];
                        if (PtInRect(new Point((int)(c.x+c.width/2), (int)(c.y+c.height/2)), currRect))
                        {
                            c.resetColor(DrawColorId);
                        }
                    }
                }
                更新撤回列表();
                在矩形渲染时 = false;
            }
        }
        /// <summary>
        /// 判断点是否在矩形框内
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static bool PtInRect(Point pt, RectangleF rect)
        {
            GraphicsPath path = new GraphicsPath();
            Region region = new Region();
            path.Reset();
            //构建多边形
            path.AddRectangle(rect);
            region.MakeEmpty();
            region.Union(path);
            //判断点是否在多边形里
            bool rlt = region.IsVisible(pt);
            region.Dispose();
            path.Dispose();
            return rlt;
        }
        /// <summary>
        /// 判断点是否在多边形内
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="pts"></param>
        /// <returns></returns>
        public static bool PtInPolygon(Point pt, Point[] pts)
        {
            GraphicsPath path = new GraphicsPath();
            Region region = new Region();
            path.Reset();
            //构建多边形
            path.AddPolygon(pts);
            region.MakeEmpty();
            region.Union(path);
            //判断点是否在多边形里
            bool rlt = region.IsVisible(pt);
            region.Dispose();
            path.Dispose();
            return rlt;
        }
        public class MessageFilter : IMessageFilter
        {

            public bool PreFilterMessage(ref Message message)
            {
                if (WIN32API.GetForegroundWindow() == This.father.Handle)
                {
                    switch (message.Msg)//判断系统消息的ID号
                    {
                        case (int)SystemMsgIds.WM_LBUTTONDOWN:
                            鼠标左键按下 = true;
                            鼠标按下位置 = PrintForm.This.PointToClient(MousePosition);
                            return false;
                        case (int)SystemMsgIds.WM_LBUTTONUP:
                            鼠标左键按下 = false;
                            鼠标松开位置 = PrintForm.This.PointToClient(MousePosition);
                            if (启用矩形渲染)
                            {
                                PrintForm.This.SetDraw();
                            }
                            else
                            {
                                更新撤回列表();
                            }
                            return false;
                        case (int)SystemMsgIds.WM_RBUTTONUP:
                            启用矩形渲染 = !启用矩形渲染;
                            return false;
                        case (int)SystemMsgIds.WM_MBUTTONUP:
                            PrintForm.This.切换颜色();
                            return false;
                        default:
                            return false;
                    }
                }
                return false;
            }
        }
        public void 切换颜色()
        {
            this.DrawColorId++;
            if(DrawColorId>=room.colors.Length)
            {
                DrawColorId = 0;
            }
        }
        private void PrintForm_MouseWheel(object sender, MouseEventArgs e)
        {
            if(e.Delta>0)
            {
                切换颜色();
            }
            else
            {
                切换颜色();
            }
        }

        private ScanerHook listener = new ScanerHook();
        public static bool 允许触发回调 = true;
        Timer t = new Timer();
        public void InitHook()
        {
            listener.Start();

            listener.ScanerEvent += Listener_ScanerEvent;//执行函数

            this.FormClosed += (sender, e) =>
            {
                listener.Stop();
            };


            t.Enabled = true;
            t.Stop();
            t.Interval = 500;
            t.Tick += new EventHandler((object s, EventArgs ea) =>
            {
                允许触发回调 = true;
            });

        }

        
        private void Listener_ScanerEvent(ScanerHook.ScanerCodes codes)
        {
            if(WIN32API.GetForegroundWindow()==this.father.Handle)
            if (允许触发回调)
            {
                string msg = codes.CurrentKey;//此处输出

                Debug.WriteLine(msg);
                允许触发回调 = false;
                t.Start();
                if(msg=="Q")
                {
                    切换颜色();
                }else if(msg=="W")
                {
                    撤回一步();
                }else if (msg=="E")
                {
                    恢复一步();
                }
            }
        }
        public void PrintForm_Load(object sender, EventArgs e)
        {
            InitHT(ref room.colors);
            this.KeyPreview = true;
            this.MouseWheel += new MouseEventHandler(PrintForm_MouseWheel);
            MainTimer.Enabled = true;
            MainTimer.Interval = 30;
            MainTimer.Tick += new EventHandler((object s, EventArgs ea) =>
            {
                Invalidate();
            });
            InitHook();
            initRoomsDraw();

            Application.AddMessageFilter(mf);
        }

        private void PrintForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.RemoveMessageFilter(mf);
        }
    }


    public class ScanerHook
    {
        public delegate void ScanerDelegate(ScanerCodes codes);
        public event ScanerDelegate ScanerEvent;
        //private const int WM_KEYDOWN = 0x100;//KEYDOWN
        //private const int WM_KEYUP = 0x101;//KEYUP
        //private const int WM_SYSKEYDOWN = 0x104;//SYSKEYDOWN
        //private const int WM_SYSKEYUP = 0x105;//SYSKEYUP

        //private static int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        private int hKeyboardHook = 0;//声明键盘钩子处理的初始值
        private ScanerCodes codes = new ScanerCodes();//13为键盘钩子

        //定义成静态，这样不会抛出回收异常
        private static HookProc hookproc;

        private delegate int HookProc(int nCode, int wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //设置钩子
        private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //卸载钩子
        private static extern bool UnhookWindowsHookEx(int idHook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        //继续下个钩子
        private static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("user32", EntryPoint = "GetKeyNameText")]
        private static extern int GetKeyNameText(int IParam, StringBuilder lpBuffer, int nSize);
        [DllImport("user32", EntryPoint = "GetKeyboardState")]
        //获取按键的状态
        private static extern int GetKeyboardState(byte[] pbKeyState);
        [DllImport("user32", EntryPoint = "ToAscii")]
        //ToAscii职能的转换指定的虚拟键码和键盘状态的相应字符或字符
        private static extern bool ToAscii(int VirtualKey, int ScanCode, byte[] lpKeySate, ref uint lpChar, int uFlags);

        //int VirtualKey //[in] 指定虚拟关键代码进行翻译。
        //int uScanCode, // [in] 指定的硬件扫描码的关键须翻译成英文。高阶位的这个值设定的关键，如果是（不压）
        //byte[] lpbKeyState, // [in] 指针，以256字节数组，包含当前键盘的状态。每个元素（字节）的数组包含状态的一个关键。如果高阶位的字节是一套，关键是下跌（按下）。在低比特，如/果设置表明，关键是对切换。在此功能，只有肘位的CAPS LOCK键是相关的。在切换状态的NUM个锁和滚动锁定键被忽略。
        //byte[] lpwTransKey, // [out] 指针的缓冲区收到翻译字符或字符。
        //uint fuState); // [in] Specifies whether a menu is active. This parameter must be 1 if a menu is active, or 0 otherwise.




        [DllImport("kernel32.dll")]
        //使用WINDOWS API函数代替获取当前实例的函数,防止钩子失效
        public static extern IntPtr GetModuleHandle(string name);

        public bool Start()
        {
            if (hKeyboardHook == 0)
            {
                hookproc = new HookProc(KeyboardHookProc);
                //GetModuleHandle 函数 替代 Marshal.GetHINSTANCE  
                //防止在 framework4.0中 注册钩子不成功  
                IntPtr modulePtr = GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
                //WH_KEYBOARD_LL=13  
                //全局钩子 WH_KEYBOARD_LL  
                //  hKeyboardHook = SetWindowsHookEx(13, hookproc, Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);  
                hKeyboardHook = SetWindowsHookEx(13, hookproc, modulePtr, 0);
            }
            return (hKeyboardHook != 0);
        }
        public bool Stop()
        {
            if (hKeyboardHook != 0)
            {
                bool retKeyboard = UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
                return retKeyboard;

            }
            return true;
        }
        private int KeyboardHookProc(int nCode, int wParam, IntPtr lParam)
        {
            EventMsg msg = (EventMsg)Marshal.PtrToStructure(lParam, typeof(EventMsg));
            codes.Add(msg);
            ScanerEvent(codes);
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }
        public class ScanerCodes
        {
            private int ts = 10; // 指定输入间隔为300毫秒以内时为连续输入  
            private List<List<EventMsg>> _keys = new List<List<EventMsg>>();
            private List<int> _keydown = new List<int>();   // 保存组合键状态  
            private List<string> _result = new List<string>();  // 返回结果集  
            private DateTime _last = DateTime.Now;
            private byte[] _state = new byte[256];
            private string _key = string.Empty;
            private string _cur = string.Empty;
            public EventMsg Event
            {
                get
                {
                    if (_keys.Count == 0)
                    {
                        return new EventMsg();
                    }
                    else
                    {
                        return _keys[_keys.Count - 1][_keys[_keys.Count - 1].Count - 1];
                    }
                }
            }
            public List<int> KeyDowns => _keydown;
            public DateTime LastInput => _last;
            public byte[] KeyboardState => _state;
            public int KeyDownCount => _keydown.Count;
            public string Result
            {
                get
                {
                    if (_result.Count > 0)
                    {
                        return _result[_result.Count - 1].Trim();
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            public string CurrentKey => _key;
            public string CurrentChar => _cur;
            public bool isShift => _keydown.Contains(160);
            public void Add(EventMsg msg)
            {
                #region 记录按键信息           

                // 首次按下按键      
                if (_keys.Count == 0)
                {
                    _keys.Add(new List<EventMsg>());
                    _keys[0].Add(msg);
                    _result.Add(string.Empty);
                }
                // 未释放其他按键时按下按键  
                else if (_keydown.Count > 0)
                {
                    _keys[_keys.Count - 1].Add(msg);
                }
                // 单位时间内按下按键  
                else if ((DateTime.Now - _last).TotalMilliseconds < ts)
                {
                    _keys[_keys.Count - 1].Add(msg);
                }
                // 从新记录输入内容  
                else
                {
                    _keys.Add(new List<EventMsg>());
                    _keys[_keys.Count - 1].Add(msg);
                    _result.Add(string.Empty);
                }
                #endregion
                _last = DateTime.Now;
                #region 获取键盘状态
                // 记录正在按下的按键  
                if (msg.paramH == 0 && !_keydown.Contains(msg.message))
                {
                    _keydown.Add(msg.message);
                }
                // 清除已松开的按键  
                if (msg.paramH > 0 && _keydown.Contains(msg.message))
                {
                    _keydown.Remove(msg.message);
                }
                #endregion
                #region 计算按键信息

                int v = msg.message & 0xff;
                int c = msg.paramL & 0xff;
                StringBuilder strKeyName = new StringBuilder(500);
                if (GetKeyNameText(c * 65536, strKeyName, 255) > 0)
                {
                    _key = strKeyName.ToString().Trim(new char[] { ' ', '\0' });
                    GetKeyboardState(_state);
                    if (_key.Length == 1 && msg.paramH == 0)// && msg.paramH == 0
                    {
                        // 根据键盘状态和shift缓存判断输出字符  
                        _cur = ShiftChar(_key, isShift, _state).ToString();
                        _result[_result.Count - 1] += _cur;
                    }
                    // 备选

                    //判断是+ 强制添加+
                    else if (_key.Length == 5 && msg.paramH == 0 && msg.paramL == 78 && msg.message == 107)// && msg.paramH == 0
                    {
                        // 根据键盘状态和shift缓存判断输出字符  
                        _cur = Convert.ToChar('+').ToString();
                        _result[_result.Count - 1] += _cur;
                    }

                    else
                    {
                        _cur = string.Empty;
                    }
                }
                #endregion
            }
            private char ShiftChar(string k, bool isShiftDown, byte[] state)
            {
                bool capslock = state[0x14] == 1;
                bool numlock = state[0x90] == 1;
                bool scrolllock = state[0x91] == 1;
                bool shiftdown = state[0xa0] == 1;
                char chr = (capslock ? k.ToUpper() : k.ToLower()).ToCharArray()[0];
                if (isShiftDown)
                {
                    if (chr >= 'a' && chr <= 'z')
                    {
                        chr = (char)(chr - 32);
                    }
                    else if (chr >= 'A' && chr <= 'Z')
                    {
                        if (chr == 'Z')
                        {
                            string s = "";
                        }
                        chr = (char)(chr + 32);
                    }
                    else
                    {
                        string s = "`1234567890-=[];',./";
                        string u = "~!@#$%^&*()_+{}:\"<>?";
                        if (s.IndexOf(chr) >= 0)
                        {
                            return (u.ToCharArray())[s.IndexOf(chr)];
                        }
                    }
                }
                return chr;
            }
        }
        public struct EventMsg
        {
            public int message;
            public int paramL;
            public int paramH;
            public int Time;
            public int hwnd;
        }
    }

    class BarCodeHook
    {
        public delegate void BarCodeDelegate(BarCodes barCode);
        public event BarCodeDelegate BarCodeEvent;
        public struct BarCodes
        {
            public int VirtKey;  //虚拟码 
            public int ScanCode;  //扫描码 
            public string KeyName; //键名 
            public uint AscII;  //AscII 
            public char Chr;   //字符
            public string BarCode; //条码信息 
            public bool IsValid;  //条码是否有效 
            public DateTime Time; //扫描时间 
        }
        private struct EventMsg
        {
            public int message;
            public int paramL;
            public int paramH;
            public int Time;
            public int hwnd;
        }
        // 安装钩子 
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        // 卸载钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern bool UnhookWindowsHookEx(int idHook);
        // 继续下一个钩子
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);
        //获取键名的字符串
        [DllImport("user32", EntryPoint = "GetKeyNameText")]
        private static extern int GetKeyNameText(int lParam, StringBuilder lpBuffer, int nSize);
        //将256个虚拟键复制到指定的缓冲区中
        [DllImport("user32", EntryPoint = "GetKeyboardState")]
        private static extern int GetKeyboardState(byte[] pbKeyState);
        //将指定的虚拟键码和键盘状态为相应的字符串
        [DllImport("user32", EntryPoint = "ToAscii")]
        private static extern bool ToAscii(int VirtualKey, int ScanCode, byte[] lpKeyState, ref uint lpChar, int uFlags);
        //声明定义回调函数
        delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        BarCodes barCode = new BarCodes();
        int hKeyboardHook = 0;
        string strBarCode = "";
        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if (nCode == 0)
            {
                EventMsg msg = (EventMsg)Marshal.PtrToStructure(lParam, typeof(EventMsg));
                if (wParam == 0x100) //WM_KEYDOWN = 0x100
                {
                    barCode.VirtKey = msg.message & 0xff; //虚拟码 
                    barCode.ScanCode = msg.paramL & 0xff; //扫描码
                    StringBuilder strKeyName = new StringBuilder(255);
                    if (GetKeyNameText(barCode.ScanCode * 65536, strKeyName, 255) > 0)
                    {
                        barCode.KeyName = strKeyName.ToString().Trim(new char[] { ' ', '\0' });
                    }
                    else
                    {
                        barCode.KeyName = "";
                    }
                    byte[] kbArray = new byte[256];
                    uint uKey = 0;
                    GetKeyboardState(kbArray);
                    if (ToAscii(barCode.VirtKey, barCode.ScanCode, kbArray, ref uKey, 0))
                    {
                        barCode.AscII = uKey;
                        barCode.Chr = Convert.ToChar(uKey);
                    }
                    if (DateTime.Now.Subtract(barCode.Time).TotalMilliseconds > 50)
                    {
                        strBarCode = barCode.Chr.ToString();
                    }
                    else
                    {
                        if ((msg.message & 0xff) == 13 && strBarCode.Length > 3)
                        //回车
                        {
                            barCode.BarCode = strBarCode;
                            barCode.IsValid = true;
                        }
                        strBarCode += barCode.Chr.ToString();
                    }
                    barCode.Time = DateTime.Now;
                    if (BarCodeEvent != null) BarCodeEvent(barCode);
                    //触发事件 
                    barCode.IsValid = false;
                }
            }
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }
        // 安装钩子 
        public bool Start()
        {
            if (hKeyboardHook == 0)
            {
                //WH_KEYBOARD_LL = 13 
                hKeyboardHook = SetWindowsHookEx(13, new HookProc(KeyboardHookProc), Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
            }
            return (hKeyboardHook != 0);
        }
        // 卸载钩子 
        public bool Stop()
        {
            if (hKeyboardHook != 0)
            {
                return UnhookWindowsHookEx(hKeyboardHook);
            }
            return true;
        }
    }
    public class WIN32API {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();
    }
    public class SystemMsgIds
    {
        public const UInt32 WM_ACTIVATE = 0x0006;
        public const UInt32 WM_ACTIVATEAPP = 0x001C;
        public const UInt32 WM_AFXFIRST = 0x0360;
        public const UInt32 WM_AFXLAST = 0x037F;
        public const UInt32 WM_APP = 0x8000;
        public const UInt32 WM_ASKCBFORMATNAME = 0x030C;
        public const UInt32 WM_CANCELJOURNAL = 0x004B;
        public const UInt32 WM_CANCELMODE = 0x001F;
        public const UInt32 WM_CAPTURECHANGED = 0x0215;
        public const UInt32 WM_CHANGECBCHAIN = 0x030D;
        public const UInt32 WM_CHANGEUISTATE = 0x0127;
        public const UInt32 WM_CHAR = 0x0102;
        public const UInt32 WM_CHARTOITEM = 0x002F;
        public const UInt32 WM_CHILDACTIVATE = 0x0022;
        public const UInt32 WM_CLEAR = 0x0303;
        public const UInt32 WM_CLOSE = 0x0010;
        public const UInt32 WM_COMMAND = 0x0111;
        public const UInt32 WM_COMPACTING = 0x0041;
        public const UInt32 WM_COMPAREITEM = 0x0039;
        public const UInt32 WM_CONTEXTMENU = 0x007B;
        public const UInt32 WM_COPY = 0x0301;
        public const UInt32 WM_COPYDATA = 0x004A;
        public const UInt32 WM_CREATE = 0x0001;
        public const UInt32 WM_CTLCOLORBTN = 0x0135;
        public const UInt32 WM_CTLCOLORDLG = 0x0136;
        public const UInt32 WM_CTLCOLOREDIT = 0x0133;
        public const UInt32 WM_CTLCOLORLISTBOX = 0x0134;
        public const UInt32 WM_CTLCOLORMSGBOX = 0x0132;
        public const UInt32 WM_CTLCOLORSCROLLBAR = 0x0137;
        public const UInt32 WM_CTLCOLORSTATIC = 0x0138;
        public const UInt32 WM_CUT = 0x0300;
        public const UInt32 WM_DEADCHAR = 0x0103;
        public const UInt32 WM_DELETEITEM = 0x002D;
        public const UInt32 WM_DESTROY = 0x0002;
        public const UInt32 WM_DESTROYCLIPBOARD = 0x0307;
        public const UInt32 WM_DEVICECHANGE = 0x0219;
        public const UInt32 WM_DEVMODECHANGE = 0x001B;
        public const UInt32 WM_DISPLAYCHANGE = 0x007E;
        public const UInt32 WM_DRAWCLIPBOARD = 0x0308;
        public const UInt32 WM_DRAWITEM = 0x002B;
        public const UInt32 WM_DROPFILES = 0x0233;
        public const UInt32 WM_ENABLE = 0x000A;
        public const UInt32 WM_ENDSESSION = 0x0016;
        public const UInt32 WM_ENTERIDLE = 0x0121;
        public const UInt32 WM_ENTERMENULOOP = 0x0211;
        public const UInt32 WM_ENTERSIZEMOVE = 0x0231;
        public const UInt32 WM_ERASEBKGND = 0x0014;
        public const UInt32 WM_EXITMENULOOP = 0x0212;
        public const UInt32 WM_EXITSIZEMOVE = 0x0232;
        public const UInt32 WM_FONTCHANGE = 0x001D;
        public const UInt32 WM_GETDLGCODE = 0x0087;
        public const UInt32 WM_GETFONT = 0x0031;
        public const UInt32 WM_GETHOTKEY = 0x0033;
        public const UInt32 WM_GETICON = 0x007F;
        public const UInt32 WM_GETMINMAXINFO = 0x0024;
        public const UInt32 WM_GETOBJECT = 0x003D;
        public const UInt32 WM_GETTEXT = 0x000D;
        public const UInt32 WM_GETTEXTLENGTH = 0x000E;
        public const UInt32 WM_HANDHELDFIRST = 0x0358;
        public const UInt32 WM_HANDHELDLAST = 0x035F;
        public const UInt32 WM_HELP = 0x0053;
        public const UInt32 WM_HOTKEY = 0x0312;
        public const UInt32 WM_HSCROLL = 0x0114;
        public const UInt32 WM_HSCROLLCLIPBOARD = 0x030E;
        public const UInt32 WM_ICONERASEBKGND = 0x0027;
        public const UInt32 WM_IME_CHAR = 0x0286;
        public const UInt32 WM_IME_COMPOSITION = 0x010F;
        public const UInt32 WM_IME_COMPOSITIONFULL = 0x0284;
        public const UInt32 WM_IME_CONTROL = 0x0283;
        public const UInt32 WM_IME_ENDCOMPOSITION = 0x010E;
        public const UInt32 WM_IME_KEYDOWN = 0x0290;
        public const UInt32 WM_IME_KEYLAST = 0x010F;
        public const UInt32 WM_IME_KEYUP = 0x0291;
        public const UInt32 WM_IME_NOTIFY = 0x0282;
        public const UInt32 WM_IME_REQUEST = 0x0288;
        public const UInt32 WM_IME_SELECT = 0x0285;
        public const UInt32 WM_IME_SETCONTEXT = 0x0281;
        public const UInt32 WM_IME_STARTCOMPOSITION = 0x010D;
        public const UInt32 WM_INITDIALOG = 0x0110;
        public const UInt32 WM_INITMENU = 0x0116;
        public const UInt32 WM_INITMENUPOPUP = 0x0117;
        public const UInt32 WM_INPUTLANGCHANGE = 0x0051;
        public const UInt32 WM_INPUTLANGCHANGEREQUEST = 0x0050;
        public const UInt32 WM_KEYDOWN = 0x0100;
        public const UInt32 WM_KEYFIRST = 0x0100;
        public const UInt32 WM_KEYLAST = 0x0108;
        public const UInt32 WM_KEYUP = 0x0101;
        public const UInt32 WM_KILLFOCUS = 0x0008;
        public const UInt32 WM_LBUTTONDBLCLK = 0x0203;
        public const UInt32 WM_LBUTTONDOWN = 0x0201;
        public const UInt32 WM_LBUTTONUP = 0x0202;
        public const UInt32 WM_MBUTTONDBLCLK = 0x0209;
        public const UInt32 WM_MBUTTONDOWN = 0x0207;
        public const UInt32 WM_MBUTTONUP = 0x0208;
        public const UInt32 WM_MDIACTIVATE = 0x0222;
        public const UInt32 WM_MDICASCADE = 0x0227;
        public const UInt32 WM_MDICREATE = 0x0220;
        public const UInt32 WM_MDIDESTROY = 0x0221;
        public const UInt32 WM_MDIGETACTIVE = 0x0229;
        public const UInt32 WM_MDIICONARRANGE = 0x0228;
        public const UInt32 WM_MDIMAXIMIZE = 0x0225;
        public const UInt32 WM_MDINEXT = 0x0224;
        public const UInt32 WM_MDIREFRESHMENU = 0x0234;
        public const UInt32 WM_MDIRESTORE = 0x0223;
        public const UInt32 WM_MDISETMENU = 0x0230;
        public const UInt32 WM_MDITILE = 0x0226;
        public const UInt32 WM_MEASUREITEM = 0x002C;
        public const UInt32 WM_MENUCHAR = 0x0120;
        public const UInt32 WM_MENUCOMMAND = 0x0126;
        public const UInt32 WM_MENUDRAG = 0x0123;
        public const UInt32 WM_MENUGETOBJECT = 0x0124;
        public const UInt32 WM_MENURBUTTONUP = 0x0122;
        public const UInt32 WM_MENUSELECT = 0x011F;
        public const UInt32 WM_MOUSEACTIVATE = 0x0021;
        public const UInt32 WM_MOUSEFIRST = 0x0200;
        public const UInt32 WM_MOUSEHOVER = 0x02A1;
        public const UInt32 WM_MOUSELAST = 0x020D;
        public const UInt32 WM_MOUSELEAVE = 0x02A3;
        public const UInt32 WM_MOUSEMOVE = 0x0200;
        public const UInt32 WM_MOUSEWHEEL = 0x020A;
        public const UInt32 WM_MOUSEHWHEEL = 0x020E;
        public const UInt32 WM_MOVE = 0x0003;
        public const UInt32 WM_MOVING = 0x0216;
        public const UInt32 WM_NCACTIVATE = 0x0086;
        public const UInt32 WM_NCCALCSIZE = 0x0083;
        public const UInt32 WM_NCCREATE = 0x0081;
        public const UInt32 WM_NCDESTROY = 0x0082;
        public const UInt32 WM_NCHITTEST = 0x0084;
        public const UInt32 WM_NCLBUTTONDBLCLK = 0x00A3;
        public const UInt32 WM_NCLBUTTONDOWN = 0x00A1;
        public const UInt32 WM_NCLBUTTONUP = 0x00A2;
        public const UInt32 WM_NCMBUTTONDBLCLK = 0x00A9;
        public const UInt32 WM_NCMBUTTONDOWN = 0x00A7;
        public const UInt32 WM_NCMBUTTONUP = 0x00A8;
        public const UInt32 WM_NCMOUSEMOVE = 0x00A0;
        public const UInt32 WM_NCPAINT = 0x0085;
        public const UInt32 WM_NCRBUTTONDBLCLK = 0x00A6;
        public const UInt32 WM_NCRBUTTONDOWN = 0x00A4;
        public const UInt32 WM_NCRBUTTONUP = 0x00A5;
        public const UInt32 WM_NEXTDLGCTL = 0x0028;
        public const UInt32 WM_NEXTMENU = 0x0213;
        public const UInt32 WM_NOTIFY = 0x004E;
        public const UInt32 WM_NOTIFYFORMAT = 0x0055;
        public const UInt32 WM_NULL = 0x0000;
        public const UInt32 WM_PAINT = 0x000F;
        public const UInt32 WM_PAINTCLIPBOARD = 0x0309;
        public const UInt32 WM_PAINTICON = 0x0026;
        public const UInt32 WM_PALETTECHANGED = 0x0311;
        public const UInt32 WM_PALETTEISCHANGING = 0x0310;
        public const UInt32 WM_PARENTNOTIFY = 0x0210;
        public const UInt32 WM_PASTE = 0x0302;
        public const UInt32 WM_PENWINFIRST = 0x0380;
        public const UInt32 WM_PENWINLAST = 0x038F;
        public const UInt32 WM_POWER = 0x0048;
        public const UInt32 WM_POWERBROADCAST = 0x0218;
        public const UInt32 WM_PRINT = 0x0317;
        public const UInt32 WM_PRINTCLIENT = 0x0318;
        public const UInt32 WM_QUERYDRAGICON = 0x0037;
        public const UInt32 WM_QUERYENDSESSION = 0x0011;
        public const UInt32 WM_QUERYNEWPALETTE = 0x030F;
        public const UInt32 WM_QUERYOPEN = 0x0013;
        public const UInt32 WM_QUEUESYNC = 0x0023;
        public const UInt32 WM_QUIT = 0x0012;
        public const UInt32 WM_RBUTTONDBLCLK = 0x0206;
        public const UInt32 WM_RBUTTONDOWN = 0x0204;
        public const UInt32 WM_RBUTTONUP = 0x0205;
        public const UInt32 WM_RENDERALLFORMATS = 0x0306;
        public const UInt32 WM_RENDERFORMAT = 0x0305;
        public const UInt32 WM_SETCURSOR = 0x0020;
        public const UInt32 WM_SETFOCUS = 0x0007;
        public const UInt32 WM_SETFONT = 0x0030;
        public const UInt32 WM_SETHOTKEY = 0x0032;
        public const UInt32 WM_SETICON = 0x0080;
        public const UInt32 WM_SETREDRAW = 0x000B;
        public const UInt32 WM_SETTEXT = 0x000C;
        public const UInt32 WM_SETTINGCHANGE = 0x001A;
        public const UInt32 WM_SHOWWINDOW = 0x0018;
        public const UInt32 WM_SIZE = 0x0005;
        public const UInt32 WM_SIZECLIPBOARD = 0x030B;
        public const UInt32 WM_SIZING = 0x0214;
        public const UInt32 WM_SPOOLERSTATUS = 0x002A;
        public const UInt32 WM_STYLECHANGED = 0x007D;
        public const UInt32 WM_STYLECHANGING = 0x007C;
        public const UInt32 WM_SYNCPAINT = 0x0088;
        public const UInt32 WM_SYSCHAR = 0x0106;
        public const UInt32 WM_SYSCOLORCHANGE = 0x0015;
        public const UInt32 WM_SYSCOMMAND = 0x0112;
        public const UInt32 WM_SYSDEADCHAR = 0x0107;
        public const UInt32 WM_SYSKEYDOWN = 0x0104;
        public const UInt32 WM_SYSKEYUP = 0x0105;
        public const UInt32 WM_TCARD = 0x0052;
        public const UInt32 WM_TIMECHANGE = 0x001E;
        public const UInt32 WM_TIMER = 0x0113;
        public const UInt32 WM_UNDO = 0x0304;
        public const UInt32 WM_UNINITMENUPOPUP = 0x0125;
        public const UInt32 WM_USER = 0x0400;
        public const UInt32 WM_USERCHANGED = 0x0054;
        public const UInt32 WM_VKEYTOITEM = 0x002E;
        public const UInt32 WM_VSCROLL = 0x0115;
        public const UInt32 WM_VSCROLLCLIPBOARD = 0x030A;
        public const UInt32 WM_WINDOWPOSCHANGED = 0x0047;
        public const UInt32 WM_WINDOWPOSCHANGING = 0x0046;
        public const UInt32 WM_WININICHANGE = 0x001A;
        public const UInt32 WM_XBUTTONDBLCLK = 0x020D;
        public const UInt32 WM_XBUTTONDOWN = 0x020B;
        public const UInt32 WM_XBUTTONUP = 0x020C;
    }
}
