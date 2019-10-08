using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using HeatMap;
using mstat.Core;

namespace mstat.Gui
{
    public partial class MainForm : Form
    {
        private int[,] _referencePoints = new int[640, 360];
        private List<PointWithValue> _points;
        private int clicks = 0;

        public MainForm()
        {
            InitializeComponent();

            WinHook.ForegroundWindowChanged += ForegroundWindowChanged;
            //WinHook.MouseChanged += MouseChanged;

            _points = new List<PointWithValue>();

            new Thread(() => {
                while (true)
                {
                    if (clicks > 0 && _points.Count > 0)
                    {
                        
                    }
                    Thread.Sleep(1000);
                }
            })
            {
                IsBackground = true
            }.Start();
        }

        private void MouseChanged(object sender, MouseMessageArg e)
        {
            if(e.Msg == 513)
            {
                clicks++;

                var x = e.X / 3;
                var y = e.Y / 3;

                var pt = new PointWithValue
                {
                    X = x,
                    Y = y,
                    Value = 1
                };


                var elem = _points.FirstOrDefault(a => a.X == pt.X && a.Y == pt.Y);
                if (elem != null)
                {
                    elem.Value++;
                }
                else
                {
                    _points.Add(pt);
                }

                var hm = new HeatMap.HeatMap
                {
                    MinValue = 1,
                    MaxValue = clicks,
                    Points = _points,
                    Width = 640,
                    Height = 360
                };

                pictureBox2.Invoke((MethodInvoker)delegate
                {
                    pictureBox2.Image = hm.Draw();
                    pictureBox2.Refresh();
                });
            }
        }

        private void ForegroundWindowChanged(object sender, ForegroundWindowChangedArg e)
        {
            label2.Text = e.ProcessName;
            pictureBox1.Image = IconHelper.GetSmallWindowIcon(e.hWnd);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ForegroundWindowChanged(null, WinHook.CreateForegroundChangedEventArg());
        }
    }
}
