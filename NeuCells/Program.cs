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
using System.Net;

namespace NeuCells
{
    internal class Program
    {
        static float[] s = new float[8];
        static float[] f = new float[8];
        public static int width, height;
        public static int sizeX, sizeY;
        public static float[,] oxmap;
        public static float[,] boxmap;
        public static bool[,] map;
        static byte[,,] frame;

        public static List<cell> cells, bcells;

        public static cell[,] cmap;
        public static food[,] fmap;

        static Random rnd = new Random();
        static SDL.SDL_Rect rect2 = new SDL.SDL_Rect();
        public static float oxygen;
        static IntPtr window, renderer;
        static int step, vismode;
        static bool running, visox, record;
        static int seed;
        static FileStream sstream;

        static List<byte> fr = new List<byte>();

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

        public struct set
        {
            public float[] input;
            public float[] answer;

            public set(float[] i, float[] a)
            {
                input = i;
                answer = a;
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

            if (record)
            {
                if (sstream != null)
                    sstream.Close();
                File.WriteAllText(".save", "");
                sstream = File.Open(".save", FileMode.Append);
                fr.Clear();
            }

            step = 0;

            cells = new List<cell>();
            cmap = new cell[width, height];
            fmap = new food[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    oxmap[x, y] = rnd.Next((int)se.GetConst("кислородом от%"), (int)se.GetConst("кислородом до%")) / 100F;
                }

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

        static void Main(string[] args)
        {
            se.Load();
            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
            window = SDL.SDL_CreateWindow("Нейронные клетки", 100, 100, 700, 501, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
            renderer = SDL.SDL_CreateRenderer(window, 0, SDL.SDL_RendererFlags.SDL_RENDERER_SOFTWARE);

            seed = (int)se.GetConst("стандартный сид");
            width = (int)se.GetConst("ширина");
            height = (int)se.GetConst("высота");
            cmap = new cell[width, height];
            fmap = new food[width, height];
            oxmap = new float[width, height];
            map = new bool[width, height];
            frame = new byte[width, height, 3];

            record = true;
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
                                string s = Microsoft.VisualBasic.Interaction.InputBox("Введите сид симуляции.\nВнимание! Это сбросит текущую симуляцию.", "Смена сида симуляции", seed.ToString());
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
                            if (record)
                            {
                                frame[x, y, 0] = 0;
                                frame[x, y, 1] = 0;
                                frame[x, y, 2] = b;
                            }
                        }
                    }
                }
                
                Array.Clear(map, 0, map.Length);
                
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
                                Random rc = new Random(cells[i].dna.genUNN);
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

                        if (record)
                        {
                            frame[cells[i].Pos.x, cells[i].Pos.y, 0] = r;
                            frame[cells[i].Pos.x, cells[i].Pos.y, 1] = g;
                            frame[cells[i].Pos.x, cells[i].Pos.y, 2] = b;
                        }

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

                        if (record)
                        {
                            frame[fd.Pos.x, fd.Pos.y, 0] = 119;
                            frame[fd.Pos.x, fd.Pos.y, 1] = 135;
                            frame[fd.Pos.x, fd.Pos.y, 2] = 153;
                        }

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
                if (record)
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            fr.Add(frame[x, y, 0]);
                            fr.Add(frame[x, y, 1]);
                            fr.Add(frame[x, y, 2]);
                        }
                    }
                    if (step % 30 == 0)
                    {
                        sstream.Write(fr.ToArray(), 0, fr.Count);
                        fr.Clear();
                    }
                }//съёмка
            }
        }

        public static class se
        {
            public static string Path = "settings";
            public static Dictionary<string, bool> cons = new Dictionary<string, bool>();
            public static Dictionary<string, float> num = new Dictionary<string, float>();
            public static Dictionary<string, float> plus = new Dictionary<string, float>();
            public static Dictionary<string, byte> ox = new Dictionary<string, byte>();
            public static Dictionary<string, int[]> intl = new Dictionary<string, int[]>();

            public static int NeuLen, FirstN, AnsN;
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

                NeuLen = se.GetInts("слои").Length;
                FirstN = se.GetInts("слои")[0];
                AnsN = se.GetInts("слои")[NeuLen - 1];
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

        public class UNN
        {
            private float[][] layers = new float[se.GetInts("слои").Length /* количество слоёв*/][];
            private float[][] errors = new float[se.GetInts("слои").Length /* количество слоёв*/][];
            private int[] neurons = se.GetInts("слои"); // количество нейронов на каждом слое
            public float[][,] weights = new float[se.GetInts("слои").Length - 1][,];

            private void ArrayInit()
            {
                for (int i = 0; i < neurons.Length; i++) // выставляем количество нейронов на каждом слое
                {
                    layers[i] = new float[neurons[i]];
                }

                for (int i = 0; i < neurons.Length; i++) // выставляем количество нейронов на каждом слое ошибок
                {
                    errors[i] = new float[neurons[i]];
                }

                for (int i = 0; i < weights.Length; i++) // выставляем количество связей
                {
                    weights[i] = new float[neurons[i], neurons[i + 1]];
                }
            }

            public UNN(DNA dna)
            {
                ArrayInit();
                rndw();
                train(dna);
            }

            public int[] th(float[] en, float[] fd, float nrj, float oxygen)
            {
                Array.Copy(en, layers[0], 8);
                Array.Copy(fd, 0, layers[0], 8, 8);
                layers[0][16] = activate(nrj);
                layers[0][17] = oxygen;
                layers[0][18] = rnd.Next(1000) / 1000;

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

            /*private void mutation(UNN nn)
            {
                bool mt = rnd.Next(100) < se.GetConst("вероятность мутации%");

                for (int i = 1; i < neurons.Length; i++)
                {
                    for (int j = 0; j < neurons[i]; j++)
                    {
                        for (int k = 0; k < neurons[i - 1]; k++)
                        {
                            if (rnd.Next(100) < se.GetConst("серьёзность мутации%") && mt)
                            {
                                weights[i - 1][k, j] = (rnd.Next(1000) - 500) / 1000.0F;
                            }
                            else
                                weights[i - 1][k, j] = nn.weights[i - 1][k, j];
                        }
                    }
                }
            }*/

            private void train(DNA dna)
            {
                foreach(set s in dna.sets)
                {
                    Array.Copy(s.input, layers[0], s.input.Length);
                    iter();
                    correct(s.answer, dna.f);
                }
            }

            private void correct(float[] ans, float f)
            {
                for (int i = 0; i < ans.Length; i++)
                {
                    errors[neurons.Length - 1][i] = ans[i] - layers[neurons.Length - 1][i];
                }

                for (int i = neurons.Length - 1; i > 0; i--)
                {
                    for (int j = 0; j < neurons[i - 1]; j++)
                    {
                        errors[i - 1][j] = 0;
                        for (int k = 0; k < neurons[i]; k++)
                        {
                            errors[i - 1][j] += errors[i][k] * weights[i - 1][j, k];
                        }
                    }
                }

                for (int i = 1; i < neurons.Length; i++)
                {
                    for (int j = 0; j < neurons[i]; j++)
                    {
                        for (int k = 0; k < neurons[i - 1]; k++)
                        {
                            float x = layers[i - 1][k];
                            float pr = 1F;
                            weights[i - 1][k, j] += f * errors[i][j] * layers[i - 1][k] * pr;
                        }
                    }
                }
            }

            static float activate(float x)
            {
                switch (se.GetConst("функция активации"))
                {
                    case 1:
                        return (float)Math.Tanh(x);
                    case 2:
                        if (x < -1)
                            return -1;
                        else if (x > 1)
                            return 1;
                        else
                            return x;
                    case 3:
                        return (float)Math.Sin(x);
                    default:
                        return x;

                }
                
            }
            //функции активации
        }

        public class DNA
        {
            public int genUNN;
            int mut;
            public int size;
            public float f = 1;
            public set[] sets;

            public DNA(cell p1, cell p2)
            {
                mut = p1.dna.mut + p2.dna.mut;
                genUNN = p1.dna.genUNN;

                f = p1.dna.f + p2.dna.f;
                size = Math.Min(p1.dna.size, p2.dna.size);
                sets = new set[size];

                for (int i = 0; i < size; i++)
                {
                    if (rnd.Next(0, 100) < se.GetConst("гены отца%"))
                        sets[i] = p1.dna.sets[i];
                    else
                        sets[i] = p2.dna.sets[i];
                }

                sets = mutation(this);

                if (mut > 10)
                {
                    genUNN = rnd.Next(int.MinValue, int.MaxValue);
                    mut = 0;
                }
            }

            set[] mutation(DNA pdna)
            {
                size = pdna.size;
                set[] Msets = new set[size];
                bool mt = rnd.Next(100) < se.GetConst("вероятность мутации%");

                float[] inp = new float[se.FirstN];
                float[] ans = new float[se.AnsN];

                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < se.FirstN; j++)
                    {
                        if (rnd.Next(100) < se.GetConst("серьёзность мутации%") && mt)
                        {
                            inp[j] = rnd.Next(1000) / 1000.0F;
                            mut++;
                        }
                        else
                            inp[j] = pdna.sets[i].input[j];
                    }

                    for (int j = 0; j < se.AnsN; j++)
                    {
                        if (rnd.Next(100) < se.GetConst("серьёзность мутации%") && mt)
                        {
                            ans[j] = rnd.Next(1000) / 1000.0F;
                            mut++;
                        }
                        else
                            ans[j] = pdna.sets[i].answer[j];
                    }

                    Msets[i] = new set(inp, ans);
                }

                return Msets;
            }

            public DNA()
            {
                mut = 0;
                genUNN = rnd.Next(int.MinValue, int.MaxValue);
                size = rnd.Next(5, 30);
                f = 1 / size;
                sets = new set[size];

                float[] inp = new float[se.FirstN];
                float[] ans = new float[se.AnsN];

                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < se.FirstN; j++)
                    {
                        inp[j] = rnd.Next(1000) / 1000.0F;
                    }

                    for (int j = 0; j < se.AnsN; j++)
                    {
                        ans[j] = rnd.Next(1000) / 1000.0F;
                    }

                    sets[i] = new set(inp, ans);
                }
            }

            public DNA(cell p)
            {
                mut = p.dna.mut;
                genUNN = p.dna.genUNN;

                f = p.dna.f;
                size = p.dna.size;
                sets = mutation(p.dna);

                if (mut > 10)
                {
                    genUNN = rnd.Next(int.MinValue, int.MaxValue);
                    mut = 0;
                }
            }
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
            public DNA dna;
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

                dna = new DNA();
                brain = new UNN(dna);
            }

            public cell(int x, int y, cell parent)
            {
                Pos.y = y;
                Pos.x = x;
                nrj = Math.Min(parent.nrj, se.GetV("сыну давать энергии", this));
                ph = parent.ph;
                time = 0;

                dna = new DNA(parent);
                brain = new UNN(dna);
            }

            public bool step()
            {
                time++;

                nrj -= 0.5F;
                oxmap[Pos.x, Pos.y] -= 0;

                if (nrj <= 0)
                {
                    fmap[Pos.x, Pos.y] = new food(Pos, se.GetV("энергия в трупе", this));
                    cmap[Pos.x, Pos.y] = null;
                    oxmap[Pos.x, Pos.y] -= se.GetV("потеря кислорода при смерти", this);
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
                            if (cmap[cx, cy].dna.genUNN == dna.genUNN)
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

                int[] cmds = brain.th(s, f, nrj, oxmap[Pos.x, Pos.y]);

                for (int i = 0; i < 2; i++)
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
                                            if (s[cmds[i] - fix] == 0.1F && ((f[cmds[i] - fix] != 1 && oxmap[Pos.x, Pos.y] > se.GetV("тратить кислород при ходьбе", this)) || (f[cmds[i] - fix] == 1 && oxmap[Pos.x, Pos.y] > se.GetV("тратить кислород при ходьбе", this) + se.GetV("тратить кислород при поедании", this))) )
                                            {
                                                Pos.x = (Pos.x + xx + width) % width;
                                                Pos.y = (Pos.y + yy + height) % height;

                                                if (f[cmds[i] - fix] == 1)
                                                {
                                                    nrj += fmap[Pos.x, Pos.y].nrj * se.GetV("получать энергии от еды", this);
                                                    oxmap[Pos.x, Pos.y] -= se.GetV("тратить кислород при поедании", this);
                                                    fmap[Pos.x, Pos.y] = null;

                                                    if (ph < 0.99F)
                                                        ph += 0.01F;
                                                }

                                                oxmap[Pos.x, Pos.y] -= se.GetV("тратить кислород при ходьбе", this);
                                                cmap[Pos.x, Pos.y] = this;
                                                cmap[(Pos.x - xx + width) % width, (Pos.y - yy + height) % height] = null;

                                                return true;
                                            }
                                        }
                                        break; //ходить
                                    case 18:
                                        {
                                            if (s[cmds[i] - fix] == 0.1F && f[cmds[i] - fix] == 0.1F && oxmap[Pos.x, Pos.y] > se.GetV("тратить кислород на размножение", this))
                                            {
                                                int cx = (Pos.x + xx + width) % width;
                                                int cy = (Pos.y + yy + height) % height;

                                                //нужно тут пофиксить а то энергия из ниоткуда для сына берётся
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
