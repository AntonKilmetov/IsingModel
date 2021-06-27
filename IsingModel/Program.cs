using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IsingModel
{
    class Model
    {
        readonly double Temperature;
        readonly double J; 
        public int[,] SpinField { get; private set; } //Конфигурация
        public int SideLength { get; private set; } // Размер решетки
        public double TotalEnergy { get; private set; } // полная энергия
        public int TotalMagnetization { get; private set; } // полная намагниченность
        public List<double> Energy { get; private set; } // Все значения энергии
        public List<double> Magnetization { get; private set; } // Все значения намагниченности
        private Random rnd = new Random();
        public Model(int sideLength, double temperature)
        {
            Temperature = temperature;
            J = temperature / 2.0;
            SideLength = sideLength;
            SpinField = new int[sideLength, sideLength];
            for (int i = 0; i < sideLength; i++)
                for (int j = 0; j < sideLength; j++)
                    SpinField[i, j] = rnd.NextDouble() < 0.5 ? 1 : -1; // Стартовая конфигурация
            CalculateStartingEnergy();
            CalculateMagnetization();
            Energy = new List<double>();
            Magnetization = new List<double>();
            Energy.Add(TotalEnergy);
            Magnetization.Add(TotalMagnetization);
        }
        /// <summary>
        /// Начальная энергия системы
        /// </summary>
        private void CalculateStartingEnergy()
        {
            for (int i = 0; i < SideLength; i++)
                for (int j = 0; j < SideLength; j++)
                    TotalEnergy += AddEnergy(i, j, 0) / 2.0;
        }
        /// <summary>
        /// Расчет намагниченности ситемы
        /// </summary>
        private void CalculateMagnetization()
        {
            TotalMagnetization = 0;
            foreach (var spin in SpinField)
                TotalMagnetization += spin;    
        }
        /// <summary>
        /// Алгоритм Метрополиса
        /// </summary>
        public void MetropolisMethod()
        {
            var i = rnd.Next(0, SideLength);
            var j = rnd.Next(0, SideLength);
            var tmpEnergy = CutEnergy(i, j);
            SpinField[i, j] *= -1;
            tmpEnergy = AddEnergy(i, j, tmpEnergy);
            var deltaE = tmpEnergy - TotalEnergy;
            var flag = false;
            if (deltaE < 0)
                flag = true;
            else if (rnd.NextDouble() < Math.Exp((-1.0 / Temperature) * deltaE))
                flag = true;
            else
            {
                SpinField[i, j] *= -1;
                Energy.Add(TotalEnergy);
                Magnetization.Add(TotalMagnetization);
            }
            if (flag)
            {
                TotalEnergy = tmpEnergy;
                CalculateMagnetization();
                Energy.Add(TotalEnergy);
                Magnetization.Add(TotalMagnetization);
            }
        }
        /// <summary>
        /// Добавляет локальную энергию после ее вырезания (CutEnergy(int i, int j))
        /// </summary>
        /// <param name="i">вертикальный узел</param>
        /// <param name="j">горизонтальный узел</param>
        /// <param name="tmpEnergy"> энергия старой конфигурации</param>
        /// <returns>новая энергия новой конфигурации</returns>
        private double AddEnergy(int i, int j, double tmpEnergy)
        {           
            tmpEnergy += -J * SpinField[i, j] * SpinField[i, (j + 1) % SideLength];
            if (j == 0)
                tmpEnergy += -J * SpinField[i, j] * SpinField[i, SideLength - 1];
            else
                tmpEnergy += -J * SpinField[i, j] * SpinField[i, j - 1];
            tmpEnergy += -J * SpinField[i, j] * SpinField[(i + 1) % SideLength, j];
            if (i == 0)
                tmpEnergy += -J * SpinField[i, j] * SpinField[SideLength - 1, j];
            else
                tmpEnergy += -J * SpinField[i, j] * SpinField[i - 1, j];
            return tmpEnergy;
        }
        /// <summary>
        /// Вырезает локальную энергию
        /// </summary>
        /// <param name="i">вертикальный узел</param>
        /// <param name="j">горизонтальный узел</param>
        /// <returns>Энергия без учета одного спина </returns>
        private double CutEnergy(int i, int j)
        {
            var tmpEnergy = TotalEnergy;
            tmpEnergy -= -J * SpinField[i, j] * SpinField[i, (j + 1) % SideLength];
            if (j == 0)
                tmpEnergy -= -J * SpinField[i, j] * SpinField[i, SideLength - 1];
            else
                tmpEnergy -= -J * SpinField[i, j] * SpinField[i, j - 1];
            tmpEnergy -= -J * SpinField[i, j] * SpinField[(i + 1) % SideLength, j];
            if (i == 0)
                tmpEnergy -= -J * SpinField[i, j] * SpinField[SideLength - 1, j];
            else
                tmpEnergy -= -J * SpinField[i, j] * SpinField[i - 1, j];
            return tmpEnergy;
        }

    }
    class Program
    {
        static void Main(string[] args)
        {
            var solution = new Model(8, 2);
            var TotalEnergy = solution.TotalEnergy;
            var TotalMagnetization = solution.TotalMagnetization;
            var SpinField = solution.SpinField;
            Console.ReadKey();
        }

        private static void ToFile(List<double> energy, List<double> magnetization)
        {
            var builder = new StringBuilder();
            foreach (var e in energy)
                builder.Append(e.ToString() + " ");
            File.WriteAllText(@"path", builder.ToString());
            builder.Clear();
            foreach (var m in magnetization)
                builder.Append(m.ToString() + " ");
            File.WriteAllText(@"path", builder.ToString());
        }
        private static void PrintField(int[,] spinField)
        {
            for (int i = 0; i < spinField.GetLength(0); i++)
            {
                for (int j = 0; j < spinField.GetLength(1); j++)
                {
                    if (spinField[i, j] == 1)
                        Console.Write("↑ ");
                    else
                        Console.Write("↓ ");
                }
                Console.WriteLine();
                Console.WriteLine();
            }
        }
    }
}
