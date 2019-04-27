using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Input;

namespace _5inArow
{
    struct Cube
    {
        public ConsoleColor clr;
        public bool isFilled;
        public Cube(ConsoleColor clr)
        {
            this.clr = clr;
            isFilled = false;
        }
    }
    struct PositionForClearing
    {
        public int x, y;
        public ConsoleColor clr;
        public PositionForClearing(int x, int y, ConsoleColor clr)
        {
            this.x = x;
            this.y = y;
            this.clr = clr;
        }
    }
    
    class Program
    {
        static public void drawCube(int x, int y, ConsoleColor clr)//рисует кубик 2 на 3 в позиции x y
        {

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Console.SetCursorPosition(x + j, y + i);
                    Console.BackgroundColor = clr;
                    Console.Write(" ");
                }
            }
        }
        static void Main(string[] args)
        {
            Game lines = new Game(10, 10);
            
            Random r = new Random();

            int progresbarSize = 25;

            while (!lines.isFull())//пока матрица не полная
            {
                //выовжу прогрессбар
                Progressbar.draw(progresbarSize);
           
                lines.InputHandler();//обрабатываю ввод управления курсором

                lines.addCubes(); //добавляю три новых кубика

                Thread.Sleep(700);

                lines.breakLine();//поиск и удаление линий из кубиков одного цвета от 5 и более

                //очищаю прогресбар
                Progressbar.clear(progresbarSize);
            }
        }
    }
}

