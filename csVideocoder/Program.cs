using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SDL2;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Security.Policy;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ScrollBar;

namespace csVideocoder
{
    internal class Program
    {
        static int width = 0, height = 0;
        static string fn = "", sfn = "";
        static IntPtr ren;

        static byte[] GetBytes(long offset, int count)
        {
            byte[] buffer = new byte[count];
            using (var fs = new FileStream(fn, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(offset, SeekOrigin.Begin);
                int bytesRead = fs.Read(buffer, 0, count);
                if (bytesRead < count)
                {
                    Array.Resize(ref buffer, bytesRead);
                }
            }
            return buffer;
        }
        static void DisplayText(int cx, int cy, int size, int interval, string display)
        {
            void Interp(string cmds, int pos)
            {
                int s = pos * interval * size;

                int[] cords = Array.ConvertAll(cmds.ToCharArray(), x => int.Parse(x.ToString()) * size);
                for (int i = 2; i < cords.Length; i += 2)
                {
                    SDL.SDL_RenderDrawLine(ren, cords[i - 2] + s + cx, cords[i - 1] + cy, cords[i] + s + cx, cords[i + 1] + cy);
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
        }

        static string UnixToTime(long time)
        {
            StringBuilder a = new StringBuilder();
            if (time / 3600 > 0)
            {
                a.Append($"{time / 3600}h ");
                time = time % 3600;
            }
            if (time / 60 > 0)
            {
                a.Append($"{time / 60}m ");
                time = time % 60;
            }
            a.Append($"{time}s");

            return a.ToString();
        }

        [STAThread]
        static void Main(string[] args)
        {
            bool running = true;
            bool play = false;
            bool saving = false;
            int ppos = 0, plen = 0, spos = 0;
            long startt = 0;

            var win = SDL.SDL_CreateWindow("Средство просмотра записей симуляций", 100, 100, 690, 501, SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL);
            ren = SDL.SDL_CreateRenderer(win, 0, SDL.SDL_RendererFlags.SDL_RENDERER_SOFTWARE);

            while (running)
            {
                while(SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1) 
                {
                    switch (e.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            running = false;
                            break;
                        case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                            int x, y;
                            SDL.SDL_GetMouseState(out x, out y);

                            if (x > 510 && x < 510 + 170 && y > 10 && y < 10 + 40)
                            {
                                OpenFileDialog OD = new OpenFileDialog();
                                OD.DefaultExt = ".save";
                                OD.Filter = "Запись симуляции|*.save";
                                if (OD.ShowDialog() == DialogResult.OK)
                                {
                                    fn = OD.FileName;
                                    var save = File.OpenRead(fn);
                                    byte[] hwnd = new byte[4];
                                    save.Read(hwnd, 0, 4);

                                    width = hwnd[0] + hwnd[1];
                                    height = hwnd[2] + hwnd[3];
                                    plen = (int)((save.Length - 4) / (width * height * 3));
                                    ppos = spos = 0;
                                    Console.WriteLine(plen);
                                }
                            }
                            if (x > 510 && x < 510 + 170 && y > 60 && y < 60 + 40 && fn != "")
                            {
                                SaveFileDialog OD = new SaveFileDialog();
                                OD.DefaultExt = ".mp4";
                                OD.Filter = "Видеофайл|*.mp4";
                                if (OD.ShowDialog() == DialogResult.OK)
                                {
                                    sfn = OD.FileName;
                                    saving = true;
                                    startt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                    
                                }
                            }
                            if (x > 510 && x < 510 + 170 && y > 110 && y < 110 + 40 && fn != "")
                            {
                                play = !play;
                            }
                            if (x > 510 && x < 510 + 170 && y > 160 && y < 160 + 40 && fn != "")
                            {
                                ppos = (ppos + (plen / 10)) % plen;
                            }
                            if (x > 510 && x < 510 + 170 && y > 210 && y < 210 + 40 && fn != "")
                            {
                                ppos = (ppos - (plen / 10) + plen) % plen;
                            }

                            break;
                    }
                }
                {
                    SDL.SDL_SetRenderDrawColor(ren, 0, 0, 0, 255);
                    SDL.SDL_RenderClear(ren);

                    SDL.SDL_SetRenderDrawColor(ren, 255, 0, 0, 255);
                    var rect = new SDL.SDL_Rect
                    {
                        x = 510,
                        y = 10,
                        w = 170,
                        h = 40,
                    };
                    SDL.SDL_RenderFillRect(ren, ref rect);

                    if (fn != "")
                        SDL.SDL_SetRenderDrawColor(ren, 0, 0, 255, 255);
                    else
                        SDL.SDL_SetRenderDrawColor(ren, 100, 0, 0, 255);

                    rect = new SDL.SDL_Rect
                    {
                        x = 510,
                        y = 60,
                        w = 170,
                        h = 40,
                    };
                    SDL.SDL_RenderFillRect(ren, ref rect);

                    if (fn != "")
                        SDL.SDL_SetRenderDrawColor(ren, 0, 100, 0, 255);
                    else
                        SDL.SDL_SetRenderDrawColor(ren, 100, 0, 0, 255);

                    rect = new SDL.SDL_Rect
                    {
                        x = 510,
                        y = 160,
                        w = 170,
                        h = 40,
                    };
                    SDL.SDL_RenderFillRect(ren, ref rect);
                    rect = new SDL.SDL_Rect
                    {
                        x = 510,
                        y = 210,
                        w = 170,
                        h = 40,
                    };
                    SDL.SDL_RenderFillRect(ren, ref rect);

                    if (play)
                        SDL.SDL_SetRenderDrawColor(ren, 0, 100, 0, 255);
                    else
                        SDL.SDL_SetRenderDrawColor(ren, 0, 0, 255, 255);

                    rect = new SDL.SDL_Rect
                    {
                        x = 510,
                        y = 110,
                        w = 170,
                        h = 40,
                    };
                    SDL.SDL_RenderFillRect(ren, ref rect);

                    SDL.SDL_SetRenderDrawColor(ren, 200, 200, 200, 255);
                    rect = new SDL.SDL_Rect
                    {
                        x = 510,
                        y = 260,
                        w = 170,
                        h = 20,
                    };
                    SDL.SDL_RenderFillRect(ren, ref rect);

                    int wd = 0;
                    if (plen != 0)
                        wd = (int)(166F * ppos / plen);

                    SDL.SDL_SetRenderDrawColor(ren, 0, 200, 0, 255);
                    rect = new SDL.SDL_Rect
                    {
                        x = 512,
                        y = 262,
                        w = wd,
                        h = 16,
                    };
                    SDL.SDL_RenderFillRect(ren, ref rect);

                    SDL.SDL_SetRenderDrawColor(ren, 255, 255, 255, 255);
                    DisplayText(515, 15, 5, 3, "open");
                    DisplayText(515, 65, 5, 3, "render");
                    DisplayText(515, 165, 5, 3, "forward");
                    DisplayText(515, 215, 5, 3, "backward");
                    if (play)
                        DisplayText(515, 115, 5, 3, "pause");
                    else
                        DisplayText(515, 115, 5, 3, "play");

                    if (saving)
                    {
                        DisplayText(510, 305, 5, 3, "saving");
                        SDL.SDL_SetRenderDrawColor(ren, 200, 200, 200, 255);
                        rect = new SDL.SDL_Rect
                        {
                            x = 510,
                            y = 345,
                            w = 170,
                            h = 20,
                        };
                        SDL.SDL_RenderFillRect(ren, ref rect);

                        long dt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - startt;

                        long rt = 0;
                        if (spos != 0)
                            rt = plen * dt / spos;

                        DisplayText(515, 370, 2, 3, $"elapsed {UnixToTime(dt)}");
                        DisplayText(515, 385, 2, 3, $"remained {UnixToTime(rt - dt)}");

                        SDL.SDL_SetRenderDrawColor(ren, 0, 200, 0, 255);
                        rect = new SDL.SDL_Rect
                        {
                            x = 512,
                            y = 347,
                            w = (int)(spos * 168F / plen),
                            h = 16,
                        };
                        SDL.SDL_RenderFillRect(ren, ref rect);
                    }
                }

                if (fn != "")
                {
                    int sizeX = 500 / width;
                    int sizeY = 500 / height;

                    int fix = 4 + (ppos * width * height * 3);
                    byte[]  frame = GetBytes(fix, width * height * 3);
                    int c = 0;
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            SDL.SDL_SetRenderDrawColor(ren, frame[c++], frame[c++], frame[c++], 255);
                            var rect = new SDL.SDL_Rect
                            {
                                x = x * sizeX,
                                y = y * sizeY,
                                w = sizeX - 1,
                                h = sizeY - 1,
                            };
                            SDL.SDL_RenderFillRect(ren, ref rect);
                        }
                    }
                }

                if (spos == plen && saving)
                {
                    saving = false;
                    string command = $"-framerate 30 -i seqence\\frame_%06d.png -c:v libx264 -pix_fmt yuv420p {sfn}";
                    RunFFmpeg("ffmpeg.exe", command);

                    string[] files = Directory.GetFiles("seqence", "*.png");
                    foreach (string file in files)
                            File.Delete(file);
                }
                else if (saving)
                {
                    Bitmap bmp = new Bitmap((width * 5 - 1) + 1, (height * 5 - 1) + 1); // Теперь размеры кратны 2
                    int fix = 4 + (spos * width * height * 3);
                    byte[] frame = GetBytes(fix, width * height * 3);
                    int c = 0;

                    // Заполнение фона черным цветом
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.Clear(Color.Black);
                    }

                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            byte r = frame[c++];
                            byte g = frame[c++];
                            byte b = frame[c++];
                            for (int xx = 0; xx < 4; xx++)
                            {
                                for (int yy = 0; yy < 4; yy++)
                                {
                                    bmp.SetPixel(x * 5 + xx, y * 5 + yy, Color.FromArgb(r, g, b));
                                }
                            }
                        }
                    }

                    string framePath = Path.Combine("seqence", $"frame_{spos:D6}.png");
                    bmp.Save(framePath, ImageFormat.Png);

                    spos++;

                }

                SDL.SDL_RenderPresent(ren);
                if (play)
                {
                    ppos = (ppos + 1) % plen;
                }
            }


        }

        static void RunFFmpeg(string ffmpegPath, string arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                using (StreamReader reader = process.StandardError)
                {
                    string result = reader.ReadToEnd();
                    Console.WriteLine(result);
                }
            }
        }
    }
}
