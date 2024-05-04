﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using SDL2;
using static NeuCells.Program;
using static SDL2.SDL;

namespace NeuCells
{
    internal class Program
    {
        static float[] s = new float[8];
        static float[] f = new float[8];
        public static int width = 100, height = 100;
        public static int sizeX, sizeY;
        public static float[,] oxmap = new float[width, height];
        public static float[,] boxmap;
        public static bool[,] map;

        public static List<cell> cells, bcells;

        public static cell[,] cmap = new cell[width, height];
        public static food[,] fmap = new food[width, height];

        static Random rnd = new Random();
        static SDL.SDL_Rect rect2 = new SDL.SDL_Rect();
        public static float oxygen;
        static IntPtr window, renderer;
        static int step, vismode;
        static bool running, visox;
        static int seed = -1;

        static StringBuilder fr;

        public struct pos
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


        static void DisplayText(pos cpos, int size, int interval, int display)
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

            SDL.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
            string text = display.ToString();
            for (int i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '0':
                        {
                            Interp("0004242000", i);
                        }
                        break;
                    case '1':
                        {
                            Interp("141001", i);
                        }
                        break;
                    case '2':
                        {
                            Interp("011021220424", i);
                        }
                        break;
                    case '3':
                        {
                            Interp("001021120212231404", i);
                        }
                        break;
                    case '4':
                        {
                            Interp("0002222420", i);
                        }
                        break;
                    case '5':
                        {
                            Interp("20000112222404", i);
                        }
                        break;
                    case '6':
                        {
                            Interp("200004242202", i);
                        }
                        break;
                    case '7':
                        {
                            Interp("0020211214", i);
                        }
                        break;
                    case '8':
                        {
                            Interp("002021030424230100", i);
                        }
                        break;
                    case '9':
                        {
                            Interp("042420000222", i);
                        }
                        break;
                }
            }
        }

        static void RandomFill()
        {
            if (seed == -1)
                seed = rnd.Next(0, int.MaxValue);

            rnd = new Random(seed);
            File.WriteAllText("save.txt", "");
            fr = new StringBuilder();

            step = 0;

            cells = new List<cell>();
            cmap = new cell[width, height];
            fmap = new food[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    oxmap[x, y] = rnd.Next(30, 90) / 100F;
                }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (rnd.Next(100) < 20)
                    {
                        cmap[x, y] = new cell(x, y);
                        cells.Add(cmap[x, y]);
                    }
                    else if (rnd.Next(100) < 30)
                        fmap[x, y] = new food(x, y, 10);
                }
            }
        }

        static void Main(string[] args)
        {
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            window = SDL.SDL_CreateWindow("Нейронные клетки", 100, 100, 700, 501, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            renderer = SDL.SDL_CreateRenderer(window, 0, SDL.SDL_RendererFlags.SDL_RENDERER_SOFTWARE);

            RandomFill();

            sizeX = 500 / width;
            sizeY = 500 / height;

            vismode = 0;
            visox = true;
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
                            }
                            if (x > 510 && x < 680 && y > 10 && y < 50)
                            {
                                visox = !visox;
                            }
                            if (x > 510 && x < 680 && y > 110 && y < 150)
                            {
                                seed = -1;
                                RandomFill();
                            }
                            if (x > 510 && x < 680 && y > 160 && y < 210)
                            {
                                string s = Microsoft.VisualBasic.Interaction.InputBox("Введите сид симуляции.\nВнимание! Это сбросит текущую симуляцию.", "Смена сида симуляции", "-1");
                                if (int.TryParse(s, out int n))
                                {
                                    seed = n;
                                    RandomFill();
                                }
                            }
                            break;
                    }
                } //UI

                SDL.SDL_SetRenderDrawColor(renderer, 1, 1, 1, 255);
                SDL.SDL_RenderClear(renderer);

                byte[,,] frame = new byte[width, height, 3];

                map = new bool[width, height];

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
                                r = (byte)(255 * cells[i].nrj / 50);
                                g = (byte)(255 * cells[i].nrj / 50);
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
                                g = 255;
                                b = (byte)(float)((float)255 * (float)cells[i].time / (float)1000);
                                break;
                        }

                        map[cells[i].Pos.x, cells[i].Pos.y] = true;

                        SDL.SDL_Rect rect = new SDL.SDL_Rect();
                        rect.x = cells[i].Pos.x * sizeX;
                        rect.y = cells[i].Pos.y * sizeY;
                        rect.h = sizeY - 1;
                        rect.w = sizeX - 1;

                        SDL_SetRenderDrawColor(renderer, r, g, b, 255);
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

                //кислородные штуки +логика круглой земли
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
                                if ((!map[x, y] && !map[(x + xx + width) % width, (y + yy + height) % height]) || (xx == 0 && yy == 0))
                                    s++;
                            }
                        }
                        float ox = oxmap[x, y] / s;
                        for (int xx = -1; xx <= 1; xx++)
                        {
                            for (int yy = -1; yy <= 1; yy++)
                            {
                                if ((!map[x, y] && !map[(x + xx + width) % width, (y + yy + height) % height]) || (xx == 0 && yy == 0))
                                    boxmap[(x + xx + width) % width, (y + yy + height) % height] += ox;
                            }
                        }
                    }
                }
                Array.Copy(boxmap, oxmap, oxmap.Length);

                {
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

                    SDL.SDL_SetRenderDrawColor(renderer, (byte)(255 * (vismode + 1) / 3), (byte)(255 * (vismode + 1) / 2), (byte)(255 * (vismode + 1) / 1), 255);
                    SDL.SDL_RenderFillRect(renderer, ref rect2);

                    {
                        rect2.x = 510;
                        rect2.y = 110;
                        rect2.w = 170;
                        rect2.h = 40;
                    }

                    SDL.SDL_SetRenderDrawColor(renderer, 255, 10, 10, 255);
                    SDL.SDL_RenderFillRect(renderer, ref rect2);

                    DisplayText(new pos(515, 160), 5, 3, seed);
                    DisplayText(new pos(515, 210), 5, 3, step);
                    DisplayText(new pos(610, 210), 5, 3, cells.Count);
                    step++;
                    DisplayText(new pos(515, 20), 5, 3, (int)(oxygen / (width * height) * 100));
                }//отрисовка интерфейса

                SDL.SDL_RenderPresent(renderer);
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            fr.Append($"{frame[x, y, 0]}/{frame[x, y, 1]}/{frame[x, y, 2]}-");
                        }
                    }
                    fr.AppendLine();

                    if (step % 30 == 0)
                    {
                        File.AppendAllText("save.txt", fr.ToString());
                        fr.Clear();
                    }
                }//съёмка
            }
        }

        public class UNN
        {
            private float[][] layers = new float[6 /* количество слоёв*/][];
            private int[] neurons = new int[6] { 18, 36, 36, 36, 36, 26 }; // количество нейронов на каждом слое
            public float[][,] weights = new float[6 - 1][,];
            public int genUNN;
            private int mut;

            private void ArrayInit()
            {
                for (int i = 0; i < neurons.Length; i++) // выставляем количество нейронов на каждом слое
                {
                    layers[i] = new float[neurons[i]];
                }

                for (int i = 0; i < weights.Length; i++) // выставляем количество связей
                {
                    weights[i] = new float[neurons[i], neurons[i + 1]];
                }
            }

            public UNN()
            {
                ArrayInit();
                rndw();
                genUNN = rnd.Next(int.MinValue, int.MaxValue);
                mut = 0;
            }

            public UNN(cell parent)
            {
                ArrayInit();
                genUNN = parent.brain.genUNN;
                mut = parent.brain.mut;
                mutation(parent.brain);

                if (mut > 128)
                {
                    genUNN = rnd.Next(int.MinValue, int.MaxValue);
                    mut = 0;
                }
            }

            public int[] th(float[] en, float[] fd, float nrj, int tm)
            {
                Array.Copy(en, layers[0], 8);
                Array.Copy(fd, 0, layers[0], 8, 8);
                layers[0][16] = activate(nrj);
                layers[0][17] = rnd.Next(1000) / 1000;

                iter();
                var res = layers[layers.Length - 1].ToList();
                int[] cmds = new int[26];
                for (int i = 0; i < res.Count; i++)
                {
                    int ind = res.IndexOf(res.Max());
                    cmds[i] = ind;
                    res[ind] = -1;
                }

                return cmds;
            }

            private void iter()
            {
                for (int i = 1; i < neurons.Length; i++)
                {
                    for (int j = 0; j < neurons[i]; j++)
                    {
                        layers[i][j] = 0;
                        for (int k = 0; k < neurons[i - 1]; k++)
                        {
                            layers[i][j] += layers[i - 1][k] * weights[i - 1][k, j];
                        }

                        layers[i][j] = activate(layers[i][j] + 1);
                    }
                }
            }

            private void rndw()
            {
                for (int i = 1; i < neurons.Length; i++)
                {
                    for (int j = 0; j < neurons[i]; j++)
                    {
                        for (int k = 0; k < neurons[i - 1]; k++)
                        {
                            weights[i - 1][k, j] = (rnd.Next(1000) - 500) / 1000.0F;
                        }
                    }
                }

            }

            private void mutation(UNN nn)
            {
                bool mt = rnd.Next(100) < 1;

                for (int i = 1; i < neurons.Length; i++)
                {
                    for (int j = 0; j < neurons[i]; j++)
                    {
                        for (int k = 0; k < neurons[i - 1]; k++)
                        {
                            if (rnd.Next(100) < 10 && mt)
                            {
                                weights[i - 1][k, j] = (rnd.Next(1000) - 500) / 1000.0F;
                                mut++;
                            }
                            else
                                weights[i - 1][k, j] = nn.weights[i - 1][k, j];
                        }
                    }
                }
            }

            static float activate(float x)
            {
                return (float)Math.Tanh(x);
            }
            //функции активации
        }

        public class food
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

        public class cell
        {
            public pos Pos;
            public UNN brain;
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

                brain = new UNN();
            }

            public cell(int x, int y, cell parent)
            {
                Pos.y = y;
                Pos.x = x;
                nrj = parent.nrj;
                ph = parent.ph;
                time = 0;

                brain = new UNN(parent);
            }

            public bool step()
            {
                time++;

                nrj -= 0.5F;
                //oxmap[Pos.x, Pos.y] -= 0.001F;

                if (nrj <= 0)
                {
                    fmap[Pos.x, Pos.y] = new food(Pos, 7);
                    cmap[Pos.x, Pos.y] = null;
                    oxmap[Pos.x, Pos.y] -= 0.05F;
                    return false;
                } //смерть

                //сканирование территории вокруг +логика круглой земли
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
                                sens = 0.5F;
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

                int[] cmds = brain.th(s, f, nrj, time);

                for (int i = 0; i < 2; i++)
                {
                    switch (cmds[i])
                    {
                        case 0:
                            //oxmap[Pos.x, Pos.y] -= 1F;
                            return true; //ждать
                        case 1:
                            if (oxmap[Pos.x, Pos.y] < 0.9F)
                            {
                                nrj += 5F * (0.9F - oxmap[Pos.x, Pos.y]);
                                oxmap[Pos.x, Pos.y] += 0.02F;
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
                                            if (s[cmds[i] - fix] != 0.1F && oxmap[Pos.x, Pos.y] > 0.05F)
                                            {

                                                int cx = (Pos.x + xx + width) % width;
                                                int cy = (Pos.y + yy + height) % height;

                                                nrj += Math.Min(cmap[cx, cy].nrj, 8) * oxmap[Pos.x, Pos.y];
                                                cmap[cx, cy].nrj -= Math.Min(cmap[cx, cy].nrj, 8);
                                                oxmap[Pos.x, Pos.y] -= 0.01F;

                                                if (ph < 0.99F)
                                                    ph += 0.01F;

                                                return true;
                                            }
                                        }
                                        break; //кусать
                                    case 10:
                                        {
                                            if (s[cmds[i] - fix] == 0.1F)
                                            {
                                                Pos.x = (Pos.x + xx + width) % width;
                                                Pos.y = (Pos.y + yy + height) % height;

                                                if (f[cmds[i] - fix] == 1 && oxmap[Pos.x, Pos.y] > 0.05F)
                                                {
                                                    nrj += fmap[Pos.x, Pos.y].nrj * oxmap[Pos.x, Pos.y];
                                                    oxmap[Pos.x, Pos.y] -= 0.01F;
                                                    fmap[Pos.x, Pos.y] = null;

                                                    if (ph < 0.99F)
                                                        ph += 0.01F;
                                                }

                                                cmap[Pos.x, Pos.y] = this;
                                                cmap[(Pos.x - xx + width) % width, (Pos.y - yy + height) % height] = null;

                                                return true;
                                            }
                                        }
                                        break; //ходить
                                    case 18:
                                        {
                                            if (s[cmds[i] - fix] == 0.1F && f[cmds[i] - fix] == 0.1F && oxmap[Pos.x, Pos.y] > 0.07)
                                            {
                                                int cx = (Pos.x + xx + width) % width;
                                                int cy = (Pos.y + yy + height) % height;

                                                nrj -= 10 * (1 - oxmap[Pos.x, Pos.y]);
                                                oxmap[Pos.x, Pos.y] -= 0.05F;
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
