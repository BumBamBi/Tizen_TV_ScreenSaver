using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Windows;
using System.Threading;

namespace TvScreensaver
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        // 시간
        static DateTime _Date = DateTime.Now;

        // 배경
        static Image[] _wallpapers = new Image[7];
        static int _nowWallpaperIndex = 0;
        static bool _init_first = true;

        // Colon
        static int _ONOFF = 1;

        // 번인 방지
        static Rectangle _stdxy = new Rectangle(650, 700, 1920, 1080);     // 기준

         // 아날로그 시계
        static DateTime _StartTime = DateTime.Now;

        static double _sec_degree = _StartTime.Second * 6;
        static double _minute_degree = _StartTime.Minute * 6 + ((double)_sec_degree / 60.0);
        static double _hour_degree = _StartTime.Hour * 30 + ((double)_minute_degree / 60.0);

        static uint _timeCnt = 0;    // 4byte -> 4,294,967,295초간 카운트 가능 -> 약 136년


        public MainPage()
        {
            InitializeComponent();
            
            // 배경 값 설정
            _wallpapers[0] = w0;
            _wallpapers[1] = w1;
            _wallpapers[2] = w2;
            _wallpapers[3] = w3;
            _wallpapers[4] = w4;
            _wallpapers[5] = w5;
            _wallpapers[6] = w6;

            // 배경 랜덤 초기화
            Random generator = new Random();
            int choice = generator.Next(0, 7);
            _nowWallpaperIndex = choice;
            _wallpapers[_nowWallpaperIndex].FadeTo(1, 1000);

            // 아날로그 시계 초기화
            sec_hand.RotateTo(_sec_degree, 1);
            minute_hand.RotateTo(_minute_degree, 1);
            hour_hand.RotateTo(_hour_degree, 1);

            // 타이머 시작
            SetTimer();
        }

        // 현재 시간 업데이트 ----------------------------------------------------------------------
        private void updateTime()
        {
            _Date = DateTime.Now;
            Device.BeginInvokeOnMainThread(() => dateS.Text = "" + _Date.ToString("ss"));
            Device.BeginInvokeOnMainThread(() => dateHM.Text = "" + _Date.ToString("HH      mm"));
            Device.BeginInvokeOnMainThread(() => dateYMD.Text = "" + _Date.ToString("yyyy / MMMM / d (ddd)"));
        }
        // -----------------------------------------------------------------------------------------

        
        // 아날로그 시계 움직이기-------------------------------------------------------------------
        private void movementAnalog()
        {
            _timeCnt += 1;

            sec_hand.RotateTo(_sec_degree + (double)(6.0 * _timeCnt), 100);        // 1초에 6도
            minute_hand.RotateTo(_minute_degree + (double)(0.1 * _timeCnt), 1);    // 1초에 0.1도
            hour_hand.RotateTo(_hour_degree + (double)(0.008333 * _timeCnt), 1);   // 1초에 0.0083도
        }
        // -----------------------------------------------------------------------------------------


        // 초 Marquee ------------------------------------------------------------------------------
        private async void MrqueeSecond()
        {
            dateS.TranslationY = 100;
            await Task.WhenAll<bool>(
                dateS.TranslateTo(0, 50, 400),
                dateS.FadeTo(1, 300)
            );
            await Task.WhenAll<bool>(
                dateS.TranslateTo(0, 0, 400),
                dateS.FadeTo(0, 300)
            );
        }
        // ----------------------------------------------------------------------------------------


        // DOT blink ------------------------------------------------------------------------------
        private void BlinkColon()
        {
            if (_ONOFF == 1)
            {
                dateDot.FadeTo(0, 1);
                _ONOFF = 0;
            }
            else
            {
                dateDot.FadeTo(1, 1);
                _ONOFF = 1;
            }
        }
        // ----------------------------------------------------------------------------------------


        // 날짜 Marquee ---------------------------------------------------------------------------
        private async void MarqueeDate(object sender, System.Timers.ElapsedEventArgs e)
        {

            double translatePosition = dateYMD.Width + dateYMD.X;

            await dateYMD.TranslateTo(-translatePosition, 0, 1000);
            dateYMD.TranslationX = translatePosition;
            await dateYMD.TranslateTo(0, 0, 1000);
        }
        // ----------------------------------------------------------------------------------------


        // 첫 59초에 실행 -------------------------------------------------------------------------
        private void StartMinLoop(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 배경변경 + 번인, 1분 타이머 실행
            myChangeWallpapers(sender, e);
            PreventionBurnin(sender, e);
            setMinTimer();
            _init_first = false;
        }

        // 매분 배경바꾸기 ------------------------------------------------------------------------
        private async void myChangeWallpapers(object sender, System.Timers.ElapsedEventArgs e)
        {
            Random generator = new Random();
            int choice = generator.Next(0, 7);

            await _wallpapers[_nowWallpaperIndex].FadeTo(0, 1000);

            for (int i = 0; i < 7; i++)
            {
                if (i == choice)
                {
                    await _wallpapers[i].FadeTo(1, 1000);
                    _nowWallpaperIndex = choice;
                    break;
                }
            }
        }


        // 번인 방지 ------------------------------------------------------------------------------
        private void PreventionBurnin(object sender, System.Timers.ElapsedEventArgs e)
        {
            Random generator = new Random();
            int pixelX;
            int pixelY;

            while (true)
            {
                
                pixelX = generator.Next(-5, 6);
                pixelY = generator.Next(-5, 6);

                if ((_stdxy.X + pixelX > 0) & (_stdxy.Y + pixelY > 300) & (_stdxy.X + pixelX < 1350) & (_stdxy.Y + pixelY < 1000) & ((pixelX != 0) | (pixelY != 0)))
                {
                    break;
                }
            }
            Rectangle Rymd = new Rectangle(dateYMD.X + pixelX, dateYMD.Y + pixelY, 1920, 1080);
            Rectangle Rdot = new Rectangle(dateDot.X + pixelX, dateDot.Y + pixelY, 1920, 1080);
            Rectangle Rhm = new Rectangle(dateHM.X + pixelX, dateHM.Y + pixelY, 1920, 1080);
            Rectangle Rs = new Rectangle(dateS.X + pixelX, dateS.Y + pixelY, 1920, 1080);

            dateYMD.LayoutTo(Rymd);
            dateHM.LayoutTo(Rhm);
            dateS.LayoutTo(Rs);
            dateDot.LayoutTo(Rdot);
        }
        //  ---------------------------------------------------------------------------------------




        // -----------------------------------------------------------------------------------------------------
        // -----------------------------------------------------------------------------------------------------



        // 매분 타이머 이벤트 핸들러 --------------------------------------------------------------
        private void setMinTimer()
        {
            // min = wallpaper
            System.Timers.Timer wallpaperTimer = new System.Timers.Timer();
            wallpaperTimer.Interval = 60000;
            wallpaperTimer.Enabled = true;
            wallpaperTimer.Elapsed += myChangeWallpapers;

            System.Timers.Timer PreventonBurnInTimer = new System.Timers.Timer();
            PreventonBurnInTimer.Interval = 60000;
            PreventonBurnInTimer.Enabled = true;
            PreventonBurnInTimer.Elapsed += PreventionBurnin;

            wallpaperTimer.Start();
            PreventonBurnInTimer.Start();
        }
        // ----------------------------------------------------------------------------------------

        // 매초 타이머 이벤트 핸들러 
        private void mySecEvent(object sender, System.Timers.ElapsedEventArgs e)
        {

            // 현재 시간 업데이트
            updateTime();

            // 초 Marquee 
            MrqueeSecond();

            // Colon blink
            BlinkColon();

            // 아날로그 시계 움직이기
            movementAnalog();

            // 첫 59초에 동작
            if (dateS.Text == "59" & _init_first)
            {
                StartMinLoop(sender, e);
            }
        }
       
        // 타이머 설정
        private void SetTimer()
        {
            // sec
            System.Timers.Timer secTimer = new System.Timers.Timer();
            secTimer.Interval = 1000;
            secTimer.Enabled = true;
            secTimer.Elapsed += mySecEvent;

            // dateMarqueeX
            System.Timers.Timer dateMarqueeXTimer = new System.Timers.Timer();
            dateMarqueeXTimer.Interval = 10000;
            dateMarqueeXTimer.Enabled = true;
            dateMarqueeXTimer.Elapsed += MarqueeDate;
                        
            secTimer.Start();
            dateMarqueeXTimer.Start();
        }

    }
}