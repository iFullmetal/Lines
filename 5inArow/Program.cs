using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;



namespace _5inArow
{
    [Serializable]
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
    [Serializable]
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
        public static bool operator !=(PositionForClearing first, PositionForClearing second)
        {
            return !(first.x == second.x && first.y == second.y && first.clr == second.clr);
        }
        public static bool operator ==(PositionForClearing first, PositionForClearing second)
        {
            return first.x == second.x && first.y == second.y && first.clr == second.clr;
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

            
            //отрисовываю рамку вокруг игровой области
            for (int i = 0; i < 30; i++)
            {
                if (i <= 20)
                {
                    Console.SetCursorPosition(30, i);
                    Console.Write("H");
                }
                Console.SetCursorPosition(i, 20);
                Console.Write("_");
            }
            //вывожу информацию об управлении игрой
            Console.SetCursorPosition(32, 4);
            Console.Write("Controls: ←, →, ↑, ↓");
            Console.SetCursorPosition(32, 5);
            Console.Write("Select cube: SPACE");
            Console.SetCursorPosition(32, 6);
            Console.Write("Select destenation: SPACE");
            Console.SetCursorPosition(32, 7);
            Console.Write("S - save");
            Console.SetCursorPosition(32, 8);
            Console.Write("L - load save");
            Console.SetCursorPosition(32, 9);
            Console.Write("Esc - exit");
            Console.SetCursorPosition(32, 12);
            Console.Write("Copyright(c) Bondarenko M.D.");

            int progresbarSize = 30; 

            while (!lines.isFull())//пока матрица не полная
            {
                //выовжу прогрессбар
                Progressbar.draw(progresbarSize);

                lines.drawScore(); //вывожу счет

                if (lines.InputHandler()) return;//обрабатываю ввод пользователя(управления курсором, перемещение кубиков с отрисовкой пути и выход из игры)

                lines.addCubes(); //добавляю три новых кубика

                Thread.Sleep(700);

                lines.breakLine();//поиск и удаление линий по горизанталям/диагоналям/вертикалям из кубиков одного цвета от 5 и более

                //очищаю прогресбар
                Progressbar.clear(progresbarSize);
            }
        }
    }
}