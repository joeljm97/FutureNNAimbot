﻿using GameOverlay.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FutureNNAimbot
{
    public class Aimbot
    {
        private GameProcess gp;
        private NeuralNet nn;
        private Settings s;
        private DrawHelper dh;
        private System.Drawing.Point coordinates;

        bool Enabled = true;

        public Aimbot(Settings settings, GameProcess gameProcess, NeuralNet neuralNet)
        {
            gp = gameProcess;
            nn = neuralNet;
            s = settings;
            dh = new DrawHelper(settings);

        }

        public void Start()
        {
            Console.WriteLine("running Aimbot :)");
            gc = new gController(s);
            bool Running = true;

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (true)
                {
                    ReadKeys();

                }
            }).Start();

            while (Running)
            {

                if (Enabled)
                {
                    coordinates = Cursor.Position;
                    var bitmap = gc.ScreenCapture(false, coordinates);
                    var items = nn.getItems(bitmap);
                    RenderItems(items);

                    dh.DrawPlaying(coordinates, s, items, Firemode);

                }
                else
                {
                    dh.DrawDisabled();
                }
            }
        }


        static bool lastMDwnState = false;
        static bool Firemode = false;
        static long lastTick = DateTime.Now.Ticks;
        private gController gc;

        //private readonly Keys[] FireKeys = new[] { Keys.LButton, Keys.RButton, Keys.Space };

        public void RenderItems(IEnumerable<Alturos.Yolo.Model.YoloItem> items)
        {

            var isMdwn = s.ShootKeys.Any(x => User32.GetAsyncKeyState(x) != 0);
            if (isMdwn || DateTime.Now.Ticks > lastTick + (s.AutoAimDelayMs * 10000)) // 10,000 ticks = 1 ms
            {
                Firemode = isMdwn || lastMDwnState;
                lastMDwnState = isMdwn;
                lastTick = DateTime.Now.Ticks;
            }

            if (items.Count() > 0 && Firemode)
            {
                Shooting(ref items);
            }
        }

        void Shooting(ref IEnumerable<Alturos.Yolo.Model.YoloItem> items)
        {
            if (s.Head)
            {
                Alturos.Yolo.Model.YoloItem nearestEnemy = items.OrderBy(x => DistanceBetweenCross(x.X + Convert.ToInt32(x.Width / 2.9) + (x.Width / 3) / 2, x.Y + (x.Height / 7) / 2)).FirstOrDefault();

                Rectangle nearestEnemyHead = Rectangle.Create(nearestEnemy.X + Convert.ToInt32(nearestEnemy.Width / 2.9), nearestEnemy.Y, Convert.ToInt32(nearestEnemy.Width / 3), nearestEnemy.Height / 7);

                if (s.SmoothAim <= 0)
                {
                    VirtualMouse.Move(Convert.ToInt32(((nearestEnemyHead.Left - s.SizeX / 2) + (nearestEnemyHead.Width / 2))),
                        Convert.ToInt32((nearestEnemyHead.Top - s.SizeY / 2 + nearestEnemyHead.Height / 3)));
                }
                else//not smoothAim
                {

                    if (s.SizeX / 2 < nearestEnemyHead.Left | s.SizeX / 2 > nearestEnemyHead.Right
                        | s.SizeY / 2 < nearestEnemyHead.Top | s.SizeY / 2 > nearestEnemyHead.Bottom)
                    {
                        VirtualMouse.Move(Convert.ToInt32(((nearestEnemyHead.Left - s.SizeX / 2) + (nearestEnemyHead.Width / 2)) * s.SmoothAim),
                            Convert.ToInt32((nearestEnemyHead.Top - s.SizeY / 2 + nearestEnemyHead.Height / 7) * s.SmoothAim));
                    }
                }
            }
            else // aim 2 body
            {

                Alturos.Yolo.Model.YoloItem nearestEnemy = items.OrderBy(x => DistanceBetweenCross(x.X + Convert.ToInt32(x.Width / 6) + (x.Width / 1.5f) / 2, x.Y + x.Height / 6 + (x.Height / 3) / 2)).First();

                Rectangle nearestEnemyBody = Rectangle.Create(nearestEnemy.X + Convert.ToInt32(nearestEnemy.Width / 6), nearestEnemy.Y + nearestEnemy.Height / 6, Convert.ToInt32(nearestEnemy.Width / 1.5f), nearestEnemy.Height / 3);
                if (s.SmoothAim <= 0)
                {
                    VirtualMouse.Move(Convert.ToInt32(((nearestEnemyBody.Left - s.SizeX / 2) + (nearestEnemyBody.Width / 2))), Convert.ToInt32((nearestEnemyBody.Top - s.SizeY / 2 + nearestEnemyBody.Height / 7)));
                }
                else
                {

                    if (s.SizeX / 2 < nearestEnemyBody.Left | s.SizeX / 2 > nearestEnemyBody.Right
                        | s.SizeY / 2 < nearestEnemyBody.Top | s.SizeY / 2 > nearestEnemyBody.Bottom)
                    {
                        VirtualMouse.Move(Convert.ToInt32(((nearestEnemyBody.Left - s.SizeX / 2) + (nearestEnemyBody.Width / 2)) * s.SmoothAim), Convert.ToInt32((nearestEnemyBody.Top - s.SizeY / 2 + nearestEnemyBody.Height / 7) * s.SmoothAim));
                    }
                }
            }

            if (s.SimpleRCS)
                VirtualMouse.Move(0, s.SimpleRCSvalue);

        }

        void ReadKeys()
        {
            if (User32.GetAsyncKeyState(Keys.F2) == -32767)
            {
                Enabled = !Enabled;
                if (Enabled)
                {
                    gc.setHandle();
                    Console.Beep(750, 100);
                    Console.Beep(1700, 200);
                }
                else
                {
                    gc.Dispose();
                    Console.Beep(1700, 100);
                    Console.Beep(750, 200);
                }
            }

            if (User32.GetAsyncKeyState(Keys.Up) == -32767)
            {
                s.SmoothAim = s.SmoothAim >= 1 ? s.SmoothAim : s.SmoothAim + 0.05f;
            }

            if (User32.GetAsyncKeyState(Keys.Down) == -32767)
            {
                s.SmoothAim = s.SmoothAim <= 0 ? s.SmoothAim : s.SmoothAim - 0.05f;
            }

            if (User32.GetAsyncKeyState(Keys.Delete) == -32767)
            {
                s.Head = s.Head == true ? false : true;
            }

            if (User32.GetAsyncKeyState(Keys.Home) == -32767)
            {
                s.SimpleRCS = s.SimpleRCS == true ? false : true;
            }

        }

        public float DistanceBetweenCross(float X, float Y)
        {
            float ydist = (Y - s.SizeY / 2);
            float xdist = (X - s.SizeX / 2);
            float Hypotenuse = (float)Math.Sqrt(Math.Pow(ydist, 2) + Math.Pow(xdist, 2));
            return Hypotenuse;
        }





    }
}
