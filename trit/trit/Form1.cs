using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace trit
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Width = 1200;
            this.Height = 550;
            InitializeDataGridView();
        }

        private void InitializeDataGridView()
        {
            dataGridView1.ColumnCount = 7;
            dataGridView1.Columns[0].Name = "number";
            dataGridView1.Columns[0].Width = 50;
            dataGridView1.Columns[1].Name = "3";
            dataGridView1.Columns[1].Width = 100;
            dataGridView1.Columns[2].Name = "9";
            dataGridView1.Columns[2].Width = 100;
            dataGridView1.Columns[3].Name = "27";
            dataGridView1.Columns[3].Width = 100;
            dataGridView1.Columns[4].Name = "crc4";
            dataGridView1.Columns[4].Width = 150;
            dataGridView1.Columns[5].Name = "crcerror";
            dataGridView1.Columns[5].Width = 150;
            dataGridView1.Columns[6].Name = "crctest";
            // 添加标题行
            //string[] titleRow = new string[] { "number","3", "9", "27", "crc4", "crcerror", "crctest" };

            dataGridView2.ColumnCount = 2;
            dataGridView2.Columns[0].Name = "HDCount";
            dataGridView2.Columns[1].Name = "falseCount";
        }

        private void Random_Click(object sender, EventArgs e)
        {
            // 获取用户输入的数字
            int count;
            if (int.TryParse(tritCount.Text, out count) && count > 0)
            {

                List<string> randomStrings = new List<string>();
                int i = 0;
                while (i < int.Parse(tritCount.Text) )
                {
                    string c = GenerateRandomSequence(int.Parse(tbrandom.Text));

                    do
                    {
                        c = GenerateRandomSequence(int.Parse(tbrandom.Text));
                    } while (randomStrings.Contains(c)); // 检查生成的字符串是否已经存在于列表中

                    randomStrings.Add(c);

                    List<int> L1 = new List<int>();
                    L1 = StringToList(c);

                    // 其他操作...

                    i++;
                }
                // 清空DataGridView的数据，除了列标题
                dataGridView1.Rows.Clear();
                List<int> L2 = new List<int>();
                GetCoef(crctrit.Text, L2);

                    i = 0;
                while (i < int.Parse(tritCount.Text))
                {

                    string c = randomStrings[i];
                    List<int> L1 = new List<int>();
                    //int asciiValue = (int)c;
                    L1 = StringToList(c);
                    int asciiValue = BalancedTernaryToDecimal(c);
                    ReverseList(L1);
                    AddZeros(L1, L2);
                    dataGridView1.Rows.Add(
                                            asciiValue,
                                            c,
                                            ToSymmetricBase(asciiValue, 9),
                                            //ListToString(L1),
                                            ToSymmetricBase27(asciiValue),
                                            GetCRC(L1, L2)
                                            ); ;
                    i++;
                    
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid positive number.");
            }
        }


        public int BalancedTernaryToDecimal(string balancedTernary)
        {
            int result = 0;
            int length = balancedTernary.Length;
            int power = 0;  // 三进制的幂次

            for (int i = length - 1; i >= 0; i--)
            {
                char currentChar = balancedTernary[i];
                if (currentChar == '1')
                {
                    result += (int)Math.Pow(3, power);
                }
                else if (currentChar == '-')
                {
                    if (i > 0 && balancedTernary[i - 1] == '1')
                    {
                        result -= (int)Math.Pow(3, power);
                        i--;  // 跳过数字1，因为 '-1' 是一个单元
                    }
                }
                power++;
            }

            return result;
        }
        public List<int> SaveL(string input)
        {
            List<int> resultList = new List<int>();
            bool negative = false; // 标记下一个数字是否应该是负数

            foreach (char c in input)
            {
                if (c == '-')
                {
                    // 遇到'-'，设置标记以使下一个数字为负数
                    negative = true;
                }
                else if (char.IsDigit(c))
                {
                    // 如果当前字符是数字，将其添加到结果列表
                    int number = (int)char.GetNumericValue(c);
                    if (negative)
                    {
                        // 如果之前的字符是'-'，将数字转换为负数
                        number *= -1;
                        negative = false; // 重置标记
                    }
                    resultList.Add(number);
                }
                else
                {
                    // 如果字符既不是'-'也不是数字，抛出异常
                    throw new ArgumentException("Input string contains invalid characters.");
                }
            }

            return resultList;
        }

        private string ToSymmetricBase(int value, int baseNum)
        {
            if (value == 0) return "0";
            StringBuilder result = new StringBuilder();
            int tempValue = Math.Abs(value);
            while (tempValue > 0)
            {
                int remainder = tempValue % baseNum;
                // 对称基数的调整
                if (baseNum == 3)
                {
                    remainder = remainder == 2 ? -1 : remainder;
                }
                else if (baseNum == 9)
                {
                    remainder = remainder > 4 ? remainder - 9 : remainder;
                }
                // 构造结果字符串，不使用"+"符号，数字间添加空格
                result.Insert(0, remainder.ToString("#;-#;0"));
                tempValue = (tempValue - Math.Abs(remainder)) / baseNum;
            }

            if (value < 0) // 如果原始数值为负
            {
                // 翻转所有符号
                for (int i = 0; i < result.Length; i++)
                {
                    if (result[i] == '+') result[i] = '-';
                    else if (result[i] == '-') result[i] = '+';
                }
            }

            // 移除最后一个空格并返回结果
            return result.ToString().Trim();
        }
        private string ToSymmetricBase27(int value)
        {
            if (value == 0) return "0";

            StringBuilder result = new StringBuilder();
            int tempValue = Math.Abs(value);
            string[] symbols = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D" };

            while (tempValue > 0)
            {
                int remainder = tempValue % 27;
                if (remainder > 13) // 如果余数超过13，使用对称负数
                {
                    remainder -= 27;
                }
                string symbol = remainder >= 0 ? symbols[remainder] : "-" + symbols[-remainder];
                result.Insert(0, symbol);
                tempValue = (tempValue - Math.Abs(remainder)) / 27;
            }

            if (value < 0) // 如果原值是负数，则翻转符号
            {
                for (int i = 0; i < result.Length; i++)
                {
                    if (result[i] == '-') result[i] = '+';
                    else if (result[i] == '+') result[i] = '-';
                }
            }

            return result.ToString().TrimEnd();
        }

        static string ListToString(List<int> list)
        {
            StringBuilder sb = new StringBuilder();
            foreach (int num in list)
            {
                sb.Append(num);
            }
            return sb.ToString();
        }

        private string GenerateRandomSequence(int randomnumber)
        {
            Random random = new Random();
            StringBuilder result = new StringBuilder();
            int count = 0;  // 记录有效数字数量

            while (count < randomnumber)
            {
                int num = random.Next(-1, 2); // 生成 -1, 0, 或 1
                if (num == -1)
                {
                    // 加入 "-1"
                    result.Append("-1");
                }
                else
                {
                    // 将 '0' 或 '1' 转换为字符并加入
                    result.Append(num.ToString());
                }
                count++;  // 只有数字才计入计数
            }

            return result.ToString();
        }


        private void GetCoef(string textBoxValue, List<int> L)
        {
            char[] charList = textBoxValue.ToCharArray();
            char sign = '+';
            int index = 0;
            int maxIndex = 0;
            foreach (char c in charList)
            {
                if (c == '-')
                {
                    sign = '-';
                    if (index + 1 < charList.Length && charList[index + 1] != 'x')
                    {
                        L[0] = -1;
                        return;
                    }
                }
                else if (c == '+')
                {
                    sign = '+';
                    if (index + 1 < charList.Length && charList[index + 1] != 'x')
                    {
                        L[0] = 1;
                        return;
                    }
                }
                else if (c == 'x')
                {
                    if (index + 1 < charList.Length && charList[index + 1] != '^')
                    {
                        L[1] = (sign == '-') ? -1 : 1;
                    }
                }

                else if (char.IsDigit(c))
                {
                    int num = c - '0';
                    while (index + 1 < charList.Length && char.IsDigit(charList[index + 1]))
                    {
                        num = num * 10 + (charList[index + 1] - '0');
                        index++;
                    }
                    maxIndex = Math.Max(maxIndex, num);
                    if (maxIndex >= L.Count)
                    {
                        L.AddRange(new int[maxIndex - L.Count + 1]);
                    }
                    L[num] = (sign == '-') ? -1 : 1;
                }
                index++;
            }
        }

        static void ReverseList(List<int> L)
        {
            int left = 0;
            int right = L.Count - 1;
            while (left < right)
            {
                int temp = L[left];
                L[left] = L[right];
                L[right] = temp;
                left++;
                right--;
            }
        }

        private void AddZeros(List<int> L1, List<int> L2)
        {
            int size = L1.Count;
            int zeros = L2.Count - 1;
            L1.AddRange(new int[zeros]);
            for (int i = size - 1; i >= 0; --i)
            {
                L1[i + zeros] = L1[i];
            }
            for (int i = 0; i < zeros; i++)
            {
                L1[i] = 0;
            }
        }

        static int IsSameSign(int I1, int I2)
        {
            return (I1 * I2 == 1) ? 1 : 0;
        }

        static void ListNegate(List<int> L)
        {
            for (int i = 0; i < L.Count; i++)
            {
                if (L[i] == 1)
                {
                    L[i] = -1;
                }
                else if (L[i] == -1)
                {
                    L[i] = 1;
                }
            }
        }

        static int Operations(int a, int b)
        {
            if (a == 1 && b == 1)
            {
                return 0;
            }
            else if (a == 1 && b == 0)
            {
                return 1;
            }
            else if (a == 1 && b == -1)
            {
                return -1;
            }
            else if (a == 0 && b == 1)
            {
                return -1;
            }
            else if (a == 0 && b == 0)
            {
                return 0;
            }
            else if (a == 0 && b == -1)
            {
                return 1;
            }
            else if (a == -1 && b == 1)
            {
                return 1;
            }
            else if (a == -1 && b == 0)
            {
                return -1;
            }
            else // if (a == -1 && b == -1)
            {
                return 0;
            }
        }
        void changeSign(List<int> L, int zeros)
        {
            for (int i = 0; i < zeros; ++i)
            {
                if (L[i] != 0)
                {
                    L[i] = -L[i];
                }
            }
        }

        public bool isAllZeros(List<int> L)
        {
            return L.All(x => x == 0);
        }

        private string GetCRC(List<int> L1, List<int> L2)
        {
            if (isAllZeros(L1))
            {
                return "00000000";
            }
            List<int> L3 = new List<int>(L1);
            int index1 = L1.Count - 1;
            while (L1[index1] == 0)
                index1--;
            int index2 = L2.Count - 1;
            int manyZeros = 0;
            int difference = index1 - index2;
            for (int a = 1; a <= difference + 1; a++)
            {
                if (L1[index1] == 0)
                {
                    if (manyZeros == 0)
                        a--;
                    index1--;
                    manyZeros = 1;
                }
                else if (IsSameSign(L1[index1], L2[index2]) == 1)
                {
                    for (int i = 0; i <= index2; i++)
                    {
                        L1[index1 - i] = Operations(L1[index1 - i], L2[index2 - i]);
                    }
                    manyZeros = 0;
                }
                else
                {
                    ListNegate(L2);
                    for (int i = 0; i <= index2; i++)
                    {
                        L1[index1 - i] = Operations(L1[index1 - i], L2[index2 - i]);
                    }
                    manyZeros = 0;
                }
            }

            int zeros = L2.Count - 1;
            for (int i = 0; i < zeros; i++)
            {
                L3[i] = L1[i];
            }
            changeSign(L3, zeros);
            return ListToString(L3);
        }
        /*
        private void RandomError_Click(object sender, EventArgs e)
        {
            Random random = new Random();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow && row.Cells[4].Value != null)
                {
                    string originalStr = row.Cells[4].Value.ToString();
                    StringBuilder newStrBuilder = new StringBuilder();
                    List<int> changedIndices = new List<int>();  // 记录改变的位置
                    int i = 0;

                    while (i < originalStr.Length)
                    {
                        string currentNumStr = originalStr[i].ToString();

                        if (currentNumStr == "-" && i + 1 < originalStr.Length && originalStr[i + 1] == '1')
                        {
                            currentNumStr += "1";
                            i++;
                        }

                        string newNumStr = currentNumStr;
                        if (random.NextDouble() < 0.3)  // 30%的概率进行更改
                        {
                            List<string> replacements = new List<string> { "-1", "0", "1" };
                            replacements.Remove(currentNumStr);
                            newNumStr = replacements[random.Next(replacements.Count)];
                            if (newNumStr != currentNumStr)  // 如果发生了改变
                            {
                                changedIndices.Add(newStrBuilder.Length);  // 记录改变的位置
                            }
                        }
                        newStrBuilder.Append(newNumStr);
                        i++;
                    }

                    string newStr = newStrBuilder.ToString();
                    row.Cells["crcerror"].Value = newStr;
                    row.Cells["crcerror"].Tag = changedIndices;  // 将改变的索引存储在 Tag 属性中
                }
            }
        }
        */
        /*
        private int currentIndex = 0; // 类成员变量，用于跟踪当前需要改变的数字的索引

        private void RandomError_Click(object sender, EventArgs e)
        {
            Random random = new Random();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow && row.Cells[4].Value != null)
                {
                    string originalStr = row.Cells[4].Value.ToString();
                    StringBuilder newStrBuilder = new StringBuilder();
                    List<int> elements = StringToList(originalStr);  // 将字符串转换为 int 列表
                    int elementCount = elements.Count;  // 计算元素数量

                    // 检查 currentIndex 是否有效
                    if (currentIndex < elementCount)
                    {
                        // 获取当前位置的元素
                        int currentElement = elements[currentIndex];
                        List<int> replacements = new List<int> { -1, 0, 1 };
                        replacements.Remove(currentElement);
                        // 随机选择一个新的数字作为替换
                        int newElement = replacements[random.Next(replacements.Count)];
                        // 替换当前元素
                        elements[currentIndex] = newElement;
                    }

                    // 重新构建字符串
                    foreach (int elem in elements)
                    {
                        if (elem == -1)
                        {
                            newStrBuilder.Append("-1");
                        }
                        else
                        {
                            newStrBuilder.Append(elem.ToString());
                        }
                    }

                    row.Cells["crcerror"].Value = newStrBuilder.ToString();
                }
            }

            // 移动到下一个索引，环绕回0
            currentIndex = (currentIndex + 1) % (int.Parse(tritCount.Text)+3);
        }

        */

        private void RandomError_Click(object sender, EventArgs e)
        {
            Random random = new Random();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow && row.Cells[4].Value != null)
                {
                    string originalStr = row.Cells[4].Value.ToString();
                    StringBuilder newStrBuilder = new StringBuilder();
                    List<int> elements = StringToList(originalStr); // 将字符串转换为 int 列表
                    int elementCount = elements.Count; // 计算元素数量

                    // 读取HDCount中的值
                    int errorCount = int.Parse(HDCount.Text); // 假设HDCount是一个有效的整数
                    HashSet<int> errorPositions = new HashSet<int>();

                    // 确保错误的数量不超过元素的总数
                    errorCount = Math.Min(errorCount, elementCount);

                    // 随机选择errorCount个不同的位置进行修改
                    while (errorPositions.Count < errorCount)
                    {
                        int position = random.Next(0, elementCount);
                        errorPositions.Add(position);
                    }

                    // 对选择的位置应用随机错误
                    for (int i = 0; i < elementCount; i++)
                    {
                        int currentElement = elements[i];
                        if (errorPositions.Contains(i))
                        {
                            List<int> replacements = new List<int> { -1, 0, 1 };
                            replacements.Remove(currentElement);
                            int newElement = replacements[random.Next(replacements.Count)];
                            elements[i] = newElement; // 替换当前元素
                        }

                        // 重新构建字符串
                        if (elements[i] == -1)
                        {
                            newStrBuilder.Append("-1");
                        }
                        else
                        {
                            newStrBuilder.Append(elements[i].ToString());
                        }
                    }

                    row.Cells["crcerror"].Value = newStrBuilder.ToString();
                }
            }
        }

        private void crctest_Click(object sender, EventArgs e)
        {
            List<int> L1 = new List<int>();
            List<int> L2 = new List<int>();
            GetCoef(crctrit.Text, L2);
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow && row.Cells["crcerror"].Value != null)
                {
                    string errorData = row.Cells["crcerror"].Value.ToString();
                    L1 = StringToList(errorData);
                    //trit.Text = errorData;
                    string Zeros = "";
                    for (int i = 1; i < L2.Count; i++)
                    {
                            Zeros = Zeros + "0";
                    }
                    List<int> L3 = new List<int>();
                    L3 = StringToList(GetCRC(L1, L2));
                    L3 = L3.Take(L2.Count-1).ToList();

                    //row.Cells["crctest"].Value = ListToString(L3);
                    //row.Cells["crctest"].Value = GetCRC(L1, L2);
                    row.Cells["crctest"].Value = CheckCRC(L3,Zeros) ? "True" : "False";
                }
            }
        }

        private bool CheckCRC(List<int> L3,string Zeros)
        {
            if (ListToString(L3) != Zeros)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public List<int> StringToList(string input)
        {
            List<int> result = new List<int>();

            for (int i = 0; i < input.Length; i++)
            {
                // 检查是否是负号
                if (input[i] == '-')
                {
                    // 确保负号后面有数字
                    if (i + 1 < input.Length && (input[i + 1] == '0' || input[i + 1] == '1'))
                    {
                        // 将负号和数字一起作为一个整体添加到列表中
                        result.Add(-1 * (input[i + 1] - '0')); // 将 '0' 或 '1' 转换为 -1 或 -0
                        i++; // 跳过下一个数字，因为我们已经处理了它
                    }
                }
                else if (input[i] == '0' || input[i] == '1')
                {
                    // 直接将 '0' 或 '1' 添加到列表中
                    result.Add(input[i] - '0'); // 将 '0' 或 '1' 转换为 0 或 1
                }
                else
                {
                    // 如果字符不是 '0'、'1' 或 '-'，抛出异常
                    throw new ArgumentException("Invalid character in input string.");
                }
            }

            return result;
        }

        private (int trueCount, int falseCount) CountTrueFalse()
        {
            int trueCount = 0;
            int falseCount = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                {
                    string value = row.Cells["crctest"].Value?.ToString();
                    if (value == "True")
                    {
                        trueCount++;
                    }
                    else if (value == "False")
                    {
                        falseCount++;
                    }
                }
            }
            return (trueCount, falseCount);
        }
        /*
        private void run_Click(object sender, EventArgs e)
        {
            int totalTrue = 0;
            int totalFalse = 0;
            for (int i = 0; i < 50; i++)
            {
                // 模拟点击Random, Randomerror, crctest按钮
                Random.PerformClick();
                RandomError.PerformClick();
                crctest.PerformClick();

                // 计数并累加
                var counts = CountTrueFalse();
                totalTrue += counts.trueCount;
                totalFalse += counts.falseCount;
            }

            // 更新 TextBox 控件
            tbtrue.Text = totalTrue.ToString();
            tbfalse.Text = totalFalse.ToString();

            // 更新图表
            UpdateChart(totalTrue, totalTrue + totalFalse);
        }
        */
        private int totalTrue = 0;
        private int totalFalse = 0;
        private void run_Click(object sender, EventArgs e)
        {
            //int hd = 1;
            for (int hd = 1; hd <= (int.Parse(tbrandom.Text) + 3); hd++)
            {
                HDCount.Text = hd.ToString();
                for (int i = 0; i < 500; i++)
                {
                    Random.PerformClick();
                    RandomError.PerformClick();
                    crctest.PerformClick();

                    //计数并累加
                    var counts = CountTrueFalse();
                    totalTrue += counts.trueCount;
                    totalFalse += counts.falseCount;
                }
                tbtrue.Text = totalTrue.ToString();
                tbfalse.Text = totalFalse.ToString();
                UpdateDGV();
                totalFalse = 0;
                totalTrue = 0;
            }

            // 更新 TextBox 控件
            //tbtrue.Text = totalTrue.ToString();
            //tbfalse.Text = totalFalse.ToString();

            // 更新图表
            //UpdateChart(totalTrue, totalFalse);
        }
        private void UpdateChart(int valueForSeries1, int valueForSeries2)
        {
            // 清除之前的点
            chart.Series["Series1"].Points.Clear();
            chart.Series["Series2"].Points.Clear();

            // 添加新的点到系列
            chart.Series["Series1"].Points.AddY(valueForSeries1);
            chart.Series["Series2"].Points.AddY(valueForSeries2);
        }
        private void UpdateDGV()
        {
            if (dataGridView2 != null && HDCount != null)
            {
                // 创建新的 DataGridViewRow 对象
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(dataGridView2);

                // 设置第一列的值为 HDCount 文本框里的内容
                row.Cells[0].Value = HDCount.Text;

                // 设置第二列的值为 falseCount 变量的值
                row.Cells[1].Value = totalTrue;

                // 将新行添加到 DataGridView2 中
                dataGridView2.Rows.Add(row);
            }
        }
        /*
        private void run2_Click(object sender, EventArgs e)
        {
            //int totalTrue = 0;
            //int totalFalse = 0;
            for (int i = 0; i < 12; i++)
            {
                // 模拟点击Random, Randomerror, crctest按钮
                RandomError.PerformClick();
                crctest.PerformClick();

                // 计数并累加
                var counts = CountTrueFalse();
                totalTrue += counts.trueCount;
                totalFalse += counts.falseCount;
            }

            // 更新 TextBox 控件
            tbtrue.Text = totalTrue.ToString();
            tbfalse.Text = totalFalse.ToString();

            // 更新图表
            //UpdateChart(totalTrue,  totalFalse);
        }
        */
    }
}
