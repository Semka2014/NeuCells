﻿using SDL2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Accord.Neuro;

namespace NeuCells
{
    public class _Random
    {
        public int inext;
        public int inextp;
        public int[] SeedArray = new int[56];

        public _Random()
            : this(Environment.TickCount)
        {
        }
        public _Random(int Seed)
        {
            int num = 161803398 - ((Seed == int.MinValue) ? int.MaxValue : Math.Abs(Seed));
            SeedArray[55] = num;
            int num2 = 1;
            for (int i = 1; i < 55; i++)
            {
                int num3 = 21 * i % 55;
                SeedArray[num3] = num2;
                num2 = num - num2;
                if (num2 < 0)
                {
                    num2 += int.MaxValue;
                }

                num = SeedArray[num3];
            }
            for (int j = 1; j < 5; j++)
            {
                for (int k = 1; k < 56; k++)
                {
                    SeedArray[k] -= SeedArray[1 + (k + 30) % 55];
                    if (SeedArray[k] < 0)
                    {
                        SeedArray[k] += int.MaxValue;
                    }
                }
            }
            inext = 0;
            inextp = 21;
        }

        protected double Sample()
        {
            return InternalSample() * 4.6566128752457969E-10;
        }

        private int InternalSample()
        {
            int num = inext;
            int num2 = inextp;
            if (++num >= 56)
            {
                num = 1;
            }

            if (++num2 >= 56)
            {
                num2 = 1;
            }

            int num3 = SeedArray[num] - SeedArray[num2];
            if (num3 == int.MaxValue)
            {
                num3--;
            }

            if (num3 < 0)
            {
                num3 += int.MaxValue;
            }

            SeedArray[num] = num3;
            inext = num;
            inextp = num2;
            return num3;
        }
        public virtual int Next()
        {
            return InternalSample();
        }

        private double GetSampleForLargeRange()
        {
            int num = InternalSample();
            if ((InternalSample() % 2 == 0) ? true : false)
            {
                num = -num;
            }
            double num2 = num;
            num2 += 2147483646.0;
            return num2 / 4294967293.0;
        }
        public int Next(int minValue, int maxValue)
        {
            long num = (long)maxValue - (long)minValue;
            if (num <= int.MaxValue)
            {
                return (int)(Sample() * (double)num) + minValue;
            }
            return (int)((long)(GetSampleForLargeRange() * (double)num) + minValue);
        }
        public int Next(int maxValue)
        {
            return (int)(Sample() * (double)maxValue);
        }

        public void SkipTo(int[] SeedArray, int inext, int inextp)
        {
            this.SeedArray = SeedArray;
            this.inext = inext;
            this.inextp = inextp;
        }
    }
    internal class Program
    {
        static float[] s = new float[8];
        static float[] f = new float[8];
        static int width, height;
        static int bitmapWidth, bitmapHeight;
        static int sizeX, sizeY;
        static float[,] oxmap;
        static float[,] boxmap;
        static byte[,,] frame;

        static List<cell> cells, bcells;

        static cell[,] cmap;
        static food[,] fmap;

        static _Random rnd = new _Random();
        static SDL.SDL_Rect rect2 = new SDL.SDL_Rect();
        static float oxygen;
        static IntPtr window, renderer;
        static int step, vismode;
        static bool running, visox, recording;
        static int seed;

        struct pos
        {
            public int x;
            public int y;

            public pos(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public static bool operator ==(pos a, pos b)
            {
                return a.x == b.x && a.y == b.y;
            }

            public static bool operator !=(pos a, pos b)
            {
                return a.x != b.x || a.y != b.y;
            }
        }

        static void DisplayText(pos cpos, int size, int interval, string display)
        {
            void Interp(string cmds, int pos)
            {
                int s = pos * interval * size;

                int[] cords = Array.ConvertAll(cmds.ToCharArray(), x => int.Parse(x.ToString()) * size);
                for (int i = 2; i < cords.Length; i += 2)
                {
                    SDL.SDL_RenderDrawLine(renderer, cords[i - 2] + s + cpos.x, cords[i - 1] + cpos.y, cords[i] + s + cpos.x, cords[i + 1] + cpos.y);
                }
            }

            string text = display;
            for (int i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '0':
                        Interp("0004242000", i);
                        break;
                    case '1':
                        Interp("141001", i);
                        break;
                    case '2':
                        Interp("011021220424", i);
                        break;
                    case '3':
                        Interp("001021120212231404", i);
                        break;
                    case '4':
                        Interp("0002222420", i);
                        break;
                    case '5':
                        Interp("20000112222404", i);
                        break;
                    case '6':
                        Interp("200004242202", i);
                        break;
                    case '7':
                        Interp("0020211214", i);
                        break;
                    case '8':
                        Interp("002021030424230100", i);
                        break;
                    case '9':
                        Interp("042420000222", i);
                        break;
                    case 'a':
                        Interp("04011021242202", i);
                        break;
                    case 'b':
                        Interp("00102112021223140400", i);
                        break;
                    case 'c':
                        Interp("20000424", i);
                        break;
                    case 'd':
                        Interp("00041423211000", i);
                        break;
                    case 'e':
                        Interp("20000222020424", i);
                        break;
                    case 'f':
                        Interp("040222020020", i);
                        break;
                    case 'g':
                        Interp("200004242212", i);
                        break;
                    case 'h':
                        Interp("000402222024", i);
                        break;
                    case 'i':
                        Interp("002010140424", i);
                        break;
                    case 'j':
                        Interp("002010140403", i);
                        break;
                    case 'k':
                        Interp("00040212201224", i);
                        break;
                    case 'l':
                        Interp("000424", i);
                        break;
                    case 'm':
                        Interp("0400122024", i);
                        break;
                    case 'n':
                        Interp("04002420", i);
                        break;
                    case 'o':
                        Interp("0004242000", i);
                        break;
                    case 'p':
                        Interp("040010211202", i);
                        break;
                    case 'q':
                        Interp("242010011222", i);
                        break;
                    case 'r':
                        Interp("040010211202122324", i);
                        break;
                    case 's':
                        Interp("211001231403", i);
                        break;
                    case 't':
                        Interp("14100020", i);
                        break;
                    case 'u':
                        Interp("00042420", i);
                        break;
                    case 'v':
                        Interp("001420", i);
                        break;
                    case 'w':
                        Interp("0004122420", i);
                        break;
                    case 'x':
                        Interp("0024122004", i);
                        break;
                    case 'y':
                        Interp("04201200", i);
                        break;
                    case 'z':
                        Interp("00200424", i);
                        break;
                    default:
                        break;
                }
            }
            return;
        }

        static void RandomFill()
        {
            if (seed == -1)
                seed = rnd.Next(int.MaxValue);

            rnd = new _Random(seed);

            string[] files = Directory.GetFiles("sequence", "*.png");
            foreach (string file in files)
            {
                File.Delete(file);
            }

            step = 0;

            cells = new List<cell>();
            cmap = new cell[width, height];
            fmap = new food[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    oxmap[x, y] = rnd.Next((int)se.GetConst("кислородом от%"), (int)se.GetConst("кислородом до%")) / 100F;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (rnd.Next(100) < se.GetConst("клетка%"))
                    {
                        cmap[x, y] = new cell(x, y);
                        cells.Add(cmap[x, y]);
                    }
                    else if (rnd.Next(100) < se.GetConst("еда%"))
                        fmap[x, y] = new food(x, y, 10);
                }
            }
        }

        static void VoidFill()
        {
            rnd = new _Random(seed);

            string[] files = Directory.GetFiles("sequence", "*.png");
            foreach (string file in files)
            {
                File.Delete(file);
            }

            cells = new List<cell>();
            cmap = new cell[width, height];
            fmap = new food[width, height];
        }

        static void SaveSimulation(string s)
        {
            StringBuilder rn = new StringBuilder();
            rn.Append(rnd.SeedArray[0]);
            for (int i = 1; i < rnd.SeedArray.Length; i++)
            {
                rn.Append($"_{rnd.SeedArray[i]}");
            }

            List<string> save = new List<string>
                                {
                                    seed.ToString(),
                                    $"{rnd.inext}_{rnd.inextp}_{rn.ToString()}",
                                    step.ToString(),
                                    $"{width}_{height}"
                                };

            foreach (food f in fmap)
            {
                if (f != null)
                    save.Add($"f_{f.Pos.x}_{f.Pos.y}_{f.nrj}");
            }
            for (int i = 0; i < cells.Count; i++)
            {
                cell c = cells.ElementAt(i);

                StringBuilder sc = new StringBuilder();
                sc.Append($"c_{c.Pos.x}_{c.Pos.y}_{c.time}_{c.ph}_{c.nrj}_{c.brain.genUNN}_{c.brain.mut}_");
                sc.Append(c.brain.GetSave());
                save.Add(sc.ToString());
            }
            for (int ox = 0; ox < width; ox++)
            {
                for (int oy = 0; oy < height; oy++)
                {
                    save.Add(oxmap[ox, oy].ToString());
                }
            }

            File.WriteAllLines(s, save);
        }

        static void LoadSimulation(string s)
        {
            string[] save = File.ReadLines(".save").ToArray();
            seed = int.Parse(save[0]);
            width = int.Parse(save[3].Split('_')[0]);
            height = int.Parse(save[3].Split('_')[1]);

            sizeX = 500 / width;
            sizeY = 500 / height;

            bitmapWidth = (width * 5) % 2 == 0 ? width * 5 : (width * 5) + 1;
            bitmapHeight = (height * 5) % 2 == 0 ? height * 5 : (height * 5) + 1;

            int[] sags = Array.ConvertAll(save[1].Split('_'), it => int.Parse(it));

            VoidFill();
            rnd.inext = sags[0];
            rnd.inextp = sags[1];
            rnd.SeedArray = sags.Skip(2).ToArray();

            step = int.Parse(save[2]);

            int st = 4;
            while (save[st][0] == 'f')
            {
                var ags = save[st].Split('_');
                int fx = int.Parse(ags[1]);
                int fy = int.Parse(ags[2]);
                float nrj = float.Parse(ags[3]);
                fmap[fx, fy] = new food(new pos(fx, fy), nrj);

                st++;
            }
            while (save[st][0] == 'c')
            {
                var ags = save[st].Split('_');
                int cx = int.Parse(ags[1]);
                int cy = int.Parse(ags[2]);
                int time = int.Parse(ags[3]);
                float ph = float.Parse(ags[4]);
                float nrj = float.Parse(ags[5]);
                int genUNN = int.Parse(ags[6]);
                int mut = int.Parse(ags[7]);

                int bt = 0;
                float[] newb = Array.ConvertAll(ags[8].Split('='), it => float.Parse(it));

                cell cl = new cell(cx, cy, ph, time, nrj, mut, genUNN, newb);

                cmap[cx, cy] = cl;
                cells.Add(cmap[cx, cy]);
                st++;
            }

            for (int ox = 0; ox < width; ox++)
            {
                for (int oy = 0; oy < height; oy++)
                {
                    oxmap[ox, oy] = float.Parse(save[st]);
                    st++;
                }
            }
        }

        static void Main(string[] args)
        {
            se.Load();
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            window = SDL.SDL_CreateWindow("Нейронные клетки", 100, 100, 690, 501, SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL);
            renderer = SDL.SDL_CreateRenderer(window, 0, SDL.SDL_RendererFlags.SDL_RENDERER_SOFTWARE);

            seed = (int)se.GetConst("стандартный сид");
            width = (int)se.GetConst("ширина");
            height = (int)se.GetConst("высота");
            cmap = new cell[width, height];
            fmap = new food[width, height];
            oxmap = new float[width, height];
            frame = new byte[width, height, 3];

            RandomFill();

            sizeX = 500 / width;
            sizeY = 500 / height;

            int bitmapWidth = (width * 5) % 2 == 0 ? width * 5 : (width * 5) + 1;
            int bitmapHeight = (height * 5) % 2 == 0 ? height * 5 : (height * 5) + 1;

            vismode = 2;
            visox = true;
            recording = false;
            running = true;

            while (running)
            {
                while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1)
                {
                    switch (e.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            running = false;
                            break;
                        case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                            int x, y;
                            SDL.SDL_GetMouseState(out x, out y);
                            if (x > 510 && x < 680 && y > 60 && y < 100)
                            {
                                vismode = (vismode + 1) % 4;
                            } //change vismod
                            if (x > 510 && x < 680 && y > 10 && y < 50)
                            {
                                visox = !visox;
                            } //on-off oxygen visualization
                            if (x > 510 && x < 680 && y > 110 && y < 140)
                            {
                                recording = !recording;
                            } //start-stop recording
                            if (x > 510 && x < 680 && y > 160 && y < 200)
                            {
                                seed = -1;
                                RandomFill();
                            } //restart
                            if (x > 510 && x < 680 && y > 210 && y < 260)
                            {
                                string s = Microsoft.VisualBasic.Interaction.InputBox("Введите сид симуляции.\nВнимание! Это сбросит текущую симуляцию.", "Смена сида симуляции", seed.ToString());
                                if (int.TryParse(s, out int n))
                                {
                                    seed = n;
                                    RandomFill();
                                }
                            } //change seed
                            if (x > 668 && x < 688 && y > 479 && y < 499)
                            {
                                string s = Microsoft.VisualBasic.Interaction.InputBox("Введите название и путь к сохранению. \nПо умолчанию сохранится в папке с программой.", "Сохранение симуляции", ".save");
                                if (s == "")
                                    break;

                                SaveSimulation(s);
                            } //save
                            if (x > 646 && x < 666 && y > 479 && y < 499)
                            {
                                string s = Microsoft.VisualBasic.Interaction.InputBox("Введите название и путь к сохранению. \nВнимание! Это сбросит текущую симуляцию.", "Открытие симуляции", ".save");
                                if (!File.Exists(s))
                                    break;

                                LoadSimulation(s);                   
                            } //load
                            break;
                    }
                } //UI events

                SDL.SDL_SetRenderDrawColor(renderer, 1, 1, 1, 255);
                SDL.SDL_RenderClear(renderer);

                if (visox)
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            byte b = (byte)(255F * (float)oxmap[x, y] / 2);
                            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, b, 255);

                            SDL.SDL_Rect rect = new SDL.SDL_Rect();
                            rect.x = x * sizeX;
                            rect.y = y * sizeY;
                            rect.h = sizeY;
                            rect.w = sizeX;

                            SDL.SDL_RenderFillRect(renderer, ref rect);
                            frame[x, y, 0] = 0;
                            frame[x, y, 1] = 0;
                            frame[x, y, 2] = b;
                        }
                    }
                }
                
                bcells = new List<cell>();
                for (int i = 0; i < cells.Count; i++)
                {
                    //int Cc = cells.Count;
                    if (cells[i].step())
                    {
                        bcells.Add(cells[i]);

                        byte r = 0, g = 0, b = 0;
                        switch (vismode)
                        {
                            case 0:
                                r = (byte)(255 * cells[i].ph);
                                g = (byte)(255 * (1F - cells[i].ph));
                                b = 0;
                                break;
                            case 1:
                                r = (byte)(255 * cells[i].nrj / 100);
                                g = (byte)(255 * cells[i].nrj / 100);
                                b = (byte)(255 * 5 / (cells[i].nrj + 1));
                                break;
                            case 2:
                                Random rc = new Random(cells[i].brain.genUNN);
                                r = (byte)(rc.Next(5, 25) * 10);
                                g = (byte)(rc.Next(5, 25) * 10);
                                b = (byte)(rc.Next(5, 25) * 10);
                                break;
                            case 3:
                                r = 255;
                                g = (byte)(255 - (float)(255 * cells[i].time / 1000));
                                b = (byte)(float)(255 * cells[i].time / 1000);
                                break;
                        }

                        SDL.SDL_Rect rect = new SDL.SDL_Rect();
                        rect.x = cells[i].Pos.x * sizeX;
                        rect.y = cells[i].Pos.y * sizeY;
                        rect.h = sizeY - 1;
                        rect.w = sizeX - 1;

                        SDL.SDL_SetRenderDrawColor(renderer, r, g, b, 255);
                        frame[cells[i].Pos.x, cells[i].Pos.y, 0] = r;
                        frame[cells[i].Pos.x, cells[i].Pos.y, 1] = g;
                        frame[cells[i].Pos.x, cells[i].Pos.y, 2] = b;

                        SDL.SDL_RenderFillRect(renderer, ref rect);
                    }

                }//инициализация и отрисовка клеток
                cells = bcells;

                SDL.SDL_SetRenderDrawColor(renderer, 119, 136, 153, 255);
                foreach (food fd in fmap)
                {
                    if (fd != null)
                    {
                        SDL.SDL_Rect rect = new SDL.SDL_Rect();
                        rect.x = fd.Pos.x * sizeX;
                        rect.y = fd.Pos.y * sizeY;
                        rect.h = sizeY - 1;
                        rect.w = sizeX - 1;

                        frame[fd.Pos.x, fd.Pos.y, 0] = 119;
                        frame[fd.Pos.x, fd.Pos.y, 1] = 135;
                        frame[fd.Pos.x, fd.Pos.y, 2] = 153;

                        SDL.SDL_RenderFillRect(renderer, ref rect);
                    }
                } //отрисовка еды

                //кислородные штуки
                oxygen = 0;
                boxmap = new float[width, height];
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        oxygen += oxmap[x, y];
                        int s = 0;
                        for (int xx = -1; xx <= 1; xx++)
                        {
                            for (int yy = -1; yy <= 1; yy++)
                            {
                                int nx = (x + xx + width) % width;
                                int ny = (y + yy + height) % height;
                                bool map = (cmap[x, y] != null || fmap[x, y] != null) && (cmap[nx, ny] != null || fmap[nx, ny] != null);
                                if ((!map && boxmap[nx, ny] < 1) || (xx == 0 && yy == 0))
                                    s++;
                            }
                        }
                        float ox = oxmap[x, y] / s;
                        for (int xx = -1; xx <= 1; xx++)
                        {
                            for (int yy = -1; yy <= 1; yy++)
                            {
                                int nx = (x + xx + width) % width;
                                int ny = (y + yy + height) % height;
                                bool map = (cmap[x, y] != null || fmap[x, y] != null) && (cmap[nx, ny] != null || fmap[nx, ny] != null);
                                if ((!map && boxmap[nx, ny] < 1) || (xx == 0 && yy == 0))
                                    boxmap[nx, ny] += ox;
                            }
                        }
                    }
                }
                Array.Copy(boxmap, oxmap, oxmap.Length);

                {
                    {
                        rect2.x = 501;
                        rect2.y = 1;
                        rect2.w = 188;
                        rect2.h = 499;
                    }

                    SDL.SDL_SetRenderDrawColor(renderer, 220, 220, 220, 255);
                    SDL.SDL_RenderFillRect(renderer, ref rect2);

                    SDL.SDL_SetRenderDrawColor(renderer, 10, 10, 200, 255);
                    {
                        rect2.x = 668;
                        rect2.y = 479;
                        rect2.w = 20;
                        rect2.h = 20;
                    }
                    SDL.SDL_RenderFillRect(renderer, ref rect2);
                    SDL.SDL_SetRenderDrawColor(renderer, 200, 10, 10, 255);
                    {
                        rect2.x = 646;
                        rect2.y = 479;
                        rect2.w = 20;
                        rect2.h = 20;
                    }
                    SDL.SDL_RenderFillRect(renderer, ref rect2);


                    {
                        rect2.x = 510;
                        rect2.y = 10;
                        rect2.w = 170;
                        rect2.h = 40;
                    }
                    byte blue = (byte)(255 * (oxygen / (width * height)));
                    byte red = 0;
                    if (255 * (oxygen / (width * height)) > 255)
                    {
                        red = 255;
                    }

                    SDL.SDL_SetRenderDrawColor(renderer, red, 0, blue, 255);
                    SDL.SDL_RenderFillRect(renderer, ref rect2);

                    {
                        rect2.x = 510;
                        rect2.y = 60;
                        rect2.w = 170;
                        rect2.h = 40;
                    }
                    
                    string cap = "", cap2;
                    switch (vismode)
                    {
                        case 0:
                            cap = "predation";
                            SDL.SDL_SetRenderDrawColor(renderer, 0, 180, 50, 255);
                            break;
                        case 1:
                            cap = "energy";
                            SDL.SDL_SetRenderDrawColor(renderer, 200, 170, 0, 255);
                            break;
                        case 2:
                            cap = "genes";
                            SDL.SDL_SetRenderDrawColor(renderer, 150, 0, 150, 255);
                            break;
                        case 3:
                            cap = "age";
                            SDL.SDL_SetRenderDrawColor(renderer, 220, 0, 150, 255);
                            break;

                    }
                    SDL.SDL_RenderFillRect(renderer, ref rect2);

                    {
                        rect2.x = 510;
                        rect2.y = 110;
                        rect2.w = 170;
                        rect2.h = 40;
                    }
                    if (recording)
                    {
                        cap2 = "stop";
                        SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 200, 255);
                    }
                    else
                    {
                        cap2 = "record";
                        SDL.SDL_SetRenderDrawColor(renderer, 0, 200, 0, 255);
                    }
                    SDL.SDL_RenderFillRect(renderer, ref rect2);

                    {
                        rect2.x = 510;
                        rect2.y = 160;
                        rect2.w = 170;
                        rect2.h = 40;
                    }

                    SDL.SDL_SetRenderDrawColor(renderer, 255, 10, 10, 255);
                    SDL.SDL_RenderFillRect(renderer, ref rect2);

                    SDL.SDL_SetRenderDrawColor(renderer, 220, 220, 220, 255);
                    DisplayText(new pos(515, 118), 5, 3, cap2);
                    DisplayText(new pos(515, 68), 5, 3, cap);

                    SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 255);

                    DisplayText(new pos(515, 210), 5, 3, seed.ToString());
                    DisplayText(new pos(515, 260), 5, 3, step.ToString());
                    DisplayText(new pos(610, 260), 5, 3, cells.Count.ToString());
                    step++;

                    SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
                    DisplayText(new pos(515, 20), 5, 3, ((int)(oxygen / (width * height) * 100)).ToString());
                    DisplayText(new pos(673, 481), 4, 0, "s");
                    DisplayText(new pos(651, 481), 4, 0, "o");
                }//отрисовка интерфейса

                SDL.SDL_RenderPresent(renderer);
                if (recording && step % se.GetConst("каждый кадр") == 0)
                {
                    Bitmap bmp = new Bitmap(bitmapWidth, bitmapHeight);

                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.Black);
                    }

                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            for (int xx = 0; xx < 4; xx++)
                            {
                                for (int yy = 0; yy < 4; yy++)
                                {
                                    bmp.SetPixel(x * 5 + xx, y * 5 + yy, Color.FromArgb(frame[x, y, 0], frame[x, y, 1], frame[x, y, 2]));
                                }
                            }
                        }
                    }

                    string framePath = Path.Combine("sequence", $"frame_{(int)(step / se.GetConst("каждый кадр")):D6}.png");
                    bmp.Save(framePath, ImageFormat.Png);
                }//съёмка
            }
        }

        static class se
        {
            public static string Path = "settings";
            public static Dictionary<string, bool> cons = new Dictionary<string, bool>();
            public static Dictionary<string, float> num = new Dictionary<string, float>();
            public static Dictionary<string, float> plus = new Dictionary<string, float>();
            public static Dictionary<string, byte> ox = new Dictionary<string, byte>();
            public static Dictionary<string, int[]> intl = new Dictionary<string, int[]>();
            public static void Load()
            {
                string[] f = File.ReadAllLines(Path);
                foreach (string s in f)
                {
                    if (s.Length != 0 && s[0] != '#')
                    {
                        string name = s.Split(':')[0];
                        string exp = s.Split(':')[1];
                        float n = float.Parse(exp.Split('+')[0]);

                        bool co = !s.Contains("(") && !s.Contains("z") && !s.Contains("x") && !exp.Contains(" ");

                        if (exp.Contains(" "))
                            intl.Add(name, Array.ConvertAll(exp.Split(' '), x => int.Parse(x)));

                        cons.Add(name, co);
                        num.Add(name, n);

                        if (!co)
                        {
                            if (exp.Contains("("))
                            {
                                plus.Add(name, float.Parse(exp.Split('(')[1].Split(')')[0]));
                            }
                            else
                                plus.Add(name, 0);

                            if (exp.Contains("z"))
                                ox.Add(name, 1);
                            else if (exp.Contains("x"))
                                ox.Add(name, 2);
                            else
                                ox.Add(name, 0);
                        }
                    }
                }
            }

            public static float GetConst(string name)
            {
                if (cons[name])
                    return num[name];
                return 0;
            }

            public static float GetV(string name, cell cl)
            {
                float V = num[name];
                if (!cons[name])
                {
                    if (ox[name] == 1)
                        V *= oxmap[cl.Pos.x, cl.Pos.y];
                    else if (ox[name] == 2)
                        V *= 1F - oxmap[cl.Pos.x, cl.Pos.y];
                    V += plus[name];
                }

                return V;
            }

            public static int[] GetInts(string name)
            {
                return intl[name];
            }

            

        }//всё для настроек

        class NN
        {
            static int[] layers = se.GetInts("слои");
            static SigmoidFunction sigmoid = new SigmoidFunction();
            ActivationNetwork network = new ActivationNetwork(sigmoid, layers[0], layers.Skip(1).ToArray());
            public int genUNN, mut;

            public string GetSave()
            {
                StringBuilder sb = new StringBuilder();

                for (int layerIndex = 0; layerIndex < network.Layers.Length; layerIndex++)
                {
                    for (int neuronIndex = 0; neuronIndex < network.Layers[layerIndex].Neurons.Length; neuronIndex++)
                    {
                        for (int weightIndex = 0; weightIndex < network.Layers[layerIndex].Neurons[neuronIndex].Weights.Length; weightIndex++)
                        {
                            double w = network.Layers[layerIndex].Neurons[neuronIndex].Weights[weightIndex];
                            if (layerIndex != 0 || neuronIndex != 0 || weightIndex != 0)
                                sb.Append($"={w}");
                            else
                                sb.Append($"{w}");
                        }
                    }
                }

                return sb.ToString();
            }

            public NN()
            {
                foreach (var layer in network.Layers)
                {
                    foreach (var neuron in layer.Neurons)
                    {
                        for (int i = 0; i < neuron.Weights.Length; i++)
                        {
                            neuron.Weights[i] = (rnd.Next(1000) - 500) / 1000.0F;
                        }
                    }
                }

                genUNN = rnd.Next(int.MinValue, int.MaxValue);
                mut = 0;
            }

            public NN(int mutt, int gen, float[] newb)
            {
                mut = mutt;
                genUNN = gen;

                int i = 0;
                for (int layerIndex = 0; layerIndex < network.Layers.Length; layerIndex++)
                {
                    for (int neuronIndex = 0; neuronIndex < network.Layers[layerIndex].Neurons.Length; neuronIndex++)
                    {
                        for (int weightIndex = 0; weightIndex < network.Layers[layerIndex].Neurons[neuronIndex].Weights.Length; weightIndex++)
                        {
                            network.Layers[layerIndex].Neurons[neuronIndex].Weights[weightIndex] = newb[i];
                            i++;
                        }
                    }
                }
            } //for save-load

            public NN(cell parent)
            {
                genUNN = parent.brain.genUNN;
                mut = parent.brain.mut;
                mutation(parent.brain);

                if (mut > 128)
                {
                    genUNN = rnd.Next(int.MinValue, int.MaxValue);
                    mut = 0;
                }
            }

            public int[] th(float[] en, float[] fd, float nrj, float oxygen)
            {
                double[] input = new double[layers[0]];
                Array.Copy(en, input, 8);
                Array.Copy(fd, 0, input, 8, 8);
                input[16] = sigmoid.Function(nrj);
                input[17] = oxygen;
                input[18] = rnd.Next(1000) / 1000;

                var res = network.Compute(input).ToList();
                int[] cmds = new int[26];
                for (int i = 0; i < res.Count; i++)
                {
                    int ind = res.IndexOf(res.Max());
                    cmds[i] = ind;
                    res[ind] = -1;
                }

                return cmds;
            }

            private void mutation(NN nn)
            {
                bool mt = rnd.Next(100) < se.GetConst("вероятность мутации%");
                //прошу прощения за эти трехэтажные адресса :)
                for (int layerIndex = 0; layerIndex < network.Layers.Length; layerIndex++)
                {
                    for (int neuronIndex = 0; neuronIndex < network.Layers[layerIndex].Neurons.Length; neuronIndex++)
                    {
                        for (int weightIndex = 0; weightIndex < network.Layers[layerIndex].Neurons[neuronIndex].Weights.Length; weightIndex++)
                        {
                            if (rnd.Next(100) < se.GetConst("серьёзность мутации%") && mt)
                            {
                                network.Layers[layerIndex].Neurons[neuronIndex].Weights[weightIndex] = (rnd.Next(1000) - 500) / 1000.0F;
                                mut++;
                            }
                            else
                                network.Layers[layerIndex].Neurons[neuronIndex].Weights[weightIndex] = nn.network.Layers[layerIndex].Neurons[neuronIndex].Weights[weightIndex];
                        }
                    }
                }
            }

            //функции активации
        }

        class food
        {
            public pos Pos;
            public float nrj;
            public food(int x, int y, float enrj)
            {
                Pos.x = x;
                Pos.y = y;
                nrj = enrj;
            }
            public food(pos P, float enrj)
            {
                Pos = P;
                nrj = enrj;
            }
        }

        class cell
        {
            public pos Pos;
            public NN brain;
            public float nrj;
            public float ph;
            public int time;

            public cell(int x, int y)
            {
                Pos.y = y;
                Pos.x = x;
                ph = 0.5F;
                nrj = 6;
                time = 0;

                brain = new NN();
            }

            public cell(int x, int y, float ph, int time, float nrj, int mut, int genUNN, float[] newb)
            {
                Pos.y = y;
                Pos.x = x;
                this.ph = ph;
                this.time = time;
                this.nrj = nrj;

                brain = new NN(mut, genUNN, newb);
            } //for save-load

            public cell(int x, int y, cell parent)
            {
                Pos.y = y;
                Pos.x = x;
                nrj = Math.Min(se.GetV("сыну давать энергии", this), parent.nrj);
                parent.nrj -= nrj;
                ph = parent.ph;
                time = 0;

                brain = new NN(parent);
            }

            public bool step()
            {
                time++;

                nrj -= se.GetV("пассивная трата энергии", this);
                oxmap[Pos.x, Pos.y] -= se.GetV("пассивное потребление кислорода", this);

                if (nrj <= 0 || (time > se.GetV("максимальный возраст", this) && se.GetV("максимальный возраст", this) != -1))
                {
                    cmap[Pos.x, Pos.y] = null;
                    float e = 0;
                    if (oxmap[Pos.x, Pos.y] > se.GetV("потеря кислорода при смерти", this)) 
                    {
                        oxmap[Pos.x, Pos.y] -= se.GetV("потеря кислорода при смерти", this);
                        e = se.GetV("энергия в трупе", this);
                    }
                    fmap[Pos.x, Pos.y] = new food(Pos, e);
                    return false;
                } //смерть

                //сканирование территории вокруг
                for (int xx = -1; xx <= 1; xx++)
                {
                    for (int yy = -1; yy <= 1; yy++)
                    {
                        int cx = (Pos.x + xx + width) % width;
                        int cy = (Pos.y + yy + height) % height;

                        float sens = 0.1F;
                        if (cmap[cx, cy] != null)
                        {
                            if (cmap[cx, cy].brain.genUNN == brain.genUNN)
                                sens = 0.3F;
                            else
                                sens = 1F;
                        }

                        switch (xx.ToString() + yy.ToString())
                        {
                            case "-1-1":
                                s[0] = sens;
                                break;
                            case "0-1":
                                s[1] = sens;
                                break;
                            case "1-1":
                                s[2] = sens;
                                break;
                            case "10":
                                s[3] = sens;
                                break;
                            case "11":
                                s[4] = sens;
                                break;
                            case "01":
                                s[5] = sens;
                                break;
                            case "-11":
                                s[6] = sens;
                                break;
                            case "-10":
                                s[7] = sens;
                                break;
                        } //нормализация
                    }
                }

                for (int xx = -1; xx <= 1; xx++)
                {
                    for (int yy = -1; yy <= 1; yy++)
                    {
                        int cx = (Pos.x + xx + width) % width;
                        int cy = (Pos.y + yy + height) % height;

                        float sens = 0.1F;
                        if (fmap[cx, cy] != null)
                            sens = 1F;

                        switch (xx.ToString() + yy.ToString())
                        {
                            case "-1-1":
                                f[0] = sens;
                                break;
                            case "0-1":
                                f[1] = sens;
                                break;
                            case "1-1":
                                f[2] = sens;
                                break;
                            case "10":
                                f[3] = sens;
                                break;
                            case "11":
                                f[4] = sens;
                                break;
                            case "01":
                                f[5] = sens;
                                break;
                            case "-11":
                                f[6] = sens;
                                break;
                            case "-10":
                                f[7] = sens;
                                break;
                        } //нормализация
                    }
                }

                int[] cmds = brain.th(s, f, nrj, oxmap[Pos.x, Pos.y]);
                
                for (int i = 0; i < 5; i++)
                {
                    switch (cmds[i])
                    {
                        case 0:
                            //oxmap[Pos.x, Pos.y] -= 1F;
                            return true; //ждать
                        case 1:
                            if (oxmap[Pos.x, Pos.y] < se.GetV("фотосинтезировать можно при кислороде меньше", this))
                            {
                                nrj += se.GetV("энергия от фотосинтеза", this);
                                oxmap[Pos.x, Pos.y] += se.GetV("вырабатывание кислорода от фотосинтеза", this);
                                if (ph > 0.01)
                                    ph -= 0.01F;
                                return true;
                            }
                            break;//фотосинтез
                        default:
                            {
                                int fix;
                                if (cmds[i] <= 9)
                                    fix = 2;
                                else if (cmds[i] <= 17)
                                    fix = 10;
                                else
                                    fix = 18;

                                int xx = 0, yy = 0;

                                switch (cmds[i] - fix)
                                {
                                    case 0:
                                        xx = -1;
                                        yy = -1;
                                        break;
                                    case 1:
                                        xx = 0;
                                        yy = -1;
                                        break;
                                    case 2:
                                        xx = 1;
                                        yy = -1;
                                        break;
                                    case 3:
                                        xx = 1;
                                        yy = 0;
                                        break;
                                    case 4:
                                        xx = 1;
                                        yy = 1;
                                        break;
                                    case 5:
                                        xx = 0;
                                        yy = 1;
                                        break;
                                    case 6:
                                        xx = -1;
                                        yy = 1;
                                        break;
                                    case 7:
                                        xx = -1;
                                        yy = 0;
                                        break;
                                } //нормализация

                                switch (fix)
                                {
                                    case 2:
                                        {
                                            if (s[cmds[i] - fix] != 0.1F && oxmap[Pos.x, Pos.y] > se.GetV("потеря кислорода при кусании", this))
                                            {

                                                int cx = (Pos.x + xx + width) % width;
                                                int cy = (Pos.y + yy + height) % height;

                                                nrj += Math.Min(cmap[cx, cy].nrj * se.GetV("получать энергии2", this), se.GetV("получать энергии", this));
                                                cmap[cx, cy].nrj -= Math.Min(cmap[cx, cy].nrj * se.GetV("забирать энергии2", this), se.GetV("забирать энергии", this));
                                                oxmap[Pos.x, Pos.y] -= se.GetV("потеря кислорода при кусании", this);

                                                if (ph < 0.99F)
                                                    ph += 0.01F;

                                                return true;
                                            }
                                        }
                                        break; //кусать
                                    case 10:
                                        {
                                            if (s[cmds[i] - fix] == 0.1F
                                                && ((f[cmds[i] - fix] != 1
                                                && oxmap[Pos.x, Pos.y] > se.GetV("тратить кислород при ходьбе", this))
                                                || (f[cmds[i] - fix] == 1
                                                && oxmap[Pos.x, Pos.y] > se.GetV("тратить кислород при ходьбе", this)
                                                + se.GetV("тратить кислород при поедании", this))) )
                                            {
                                                int Nx = (Pos.x + xx + width) % width;
                                                int Ny = (Pos.y + yy + height) % height;

                                                if (f[cmds[i] - fix] == 1)
                                                {
                                                    nrj += fmap[Nx, Ny].nrj * se.GetV("получать энергии от еды", this);
                                                    oxmap[Pos.x, Pos.y] -= se.GetV("тратить кислород при поедании", this);
                                                    fmap[Nx, Ny] = null;

                                                    if (ph < 0.99F)
                                                        ph += 0.01F;
                                                }

                                                oxmap[Pos.x, Pos.y] -= se.GetV("тратить кислород при ходьбе", this);
                                                cmap[Nx, Ny] = this;
                                                cmap[Pos.x, Pos.y] = null;

                                                Pos.x = Nx;
                                                Pos.y = Ny;
                                                return true;
                                            }
                                        }
                                        break; //ходить
                                    case 18:
                                        {
                                            if (s[cmds[i] - fix] == 0.1F && f[cmds[i] - fix] == 0.1F && oxmap[Pos.x, Pos.y] > se.GetV("тратить кислород на размножение", this) && nrj > se.GetV("тратить энергии на размножение", this) + 1)
                                            {
                                                int cx = (Pos.x + xx + width) % width;
                                                int cy = (Pos.y + yy + height) % height;

                                                nrj -= se.GetV("тратить энергии на размножение", this);
                                                oxmap[Pos.x, Pos.y] -= se.GetV("тратить кислород на размножение", this);
                                                cmap[cx, cy] = new cell(cx, cy, this);
                                                bcells.Add(cmap[cx, cy]);
                                                return true;
                                            }
                                        }
                                        break; //размножаться
                                }
                            }
                            break;
                    }
                }

                return true;
            }

        }
    }
}