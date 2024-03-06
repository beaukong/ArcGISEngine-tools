using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using MyArcEngineMethod;
namespace TrajecProcessTool
{
    public partial class ParameterSetting : Form
    {
        //G:\LiYing\testdata\Wuhan20151124_1-1.txt
        //G:\LiYing\testdata\test.shp
        public int mode
        {
            get;
            set;
        }
        public int myTimer
        {
            get;
            set;
        }

        public ParameterSetting(int mode)
        {
            this.mode = mode;
            InitializeComponent();
            if (mode == 1)
            {
                this.textBox4.Hide();
                this.label4.Hide();
                this.label5.Hide();
                this.button5.Hide();
            }
            if (mode == 2)
            {
                this.textBox1.Hide();
                this.textBox3.Hide();
                this.label1.Hide();
                this.label3.Hide();
                this.label5.Hide();
                this.label7.Visible = false;
                this.button3.Hide();
            }
            this.button1.Enabled = false;
            this.button2.Enabled = false;
        }
        public ParameterSetting()
        {

            InitializeComponent();
        }
        //取消
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //OK 确认
        private void button1_Click(object sender, EventArgs e)
        {

            //获取到文本框中的路径
            string txtPath = this.textBox1.Text;
            string poiPath = this.textBox2.Text;
            string interval = this.textBox3.Text;
            string linePath = this.textBox4.Text;
            int inter;
            //kb 将字符串转换为整型，成功：得到整型；失败得到0.
            int.TryParse(interval, out inter);
            if (this.mode == 1)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                //判断txt文件是否存在
                if (!File.Exists(txtPath))
                {
                    MessageBox.Show("Txt文件不存在，请检查路径！");
                    return;
                }
                //判断shp路径是否以shp结尾//kb 可以用MapGIS中过滤可打开的文件类型。
                if (!poiPath.EndsWith(".shp"))
                {
                    MessageBox.Show("shp路径输入有误，请检查！");
                    return;
                }
                //解析txt文件
                List<string> txtContent = new System.Collections.Generic.List<string>();//kb 与new System.Collections.Generic.List<string>()一样
                StreamReader sr = new StreamReader(txtPath);
                string line = "";
                line = sr.ReadLine();
                if (line != "Time,VehiID,Lon,Lat")
                {
                    MessageBox.Show("Txt内容有误，读取到的第一行为：{0}", line);
                    return;
                }
                while ((line = sr.ReadLine()) != null)
                {
                    txtContent.Add(line);
                }
                sr.Close();

                //新建一个点shp文件
                IFeatureClass coorShpClass = null;
                string coorShpPath;
                try
                {
                    string prjPath = System.Environment.CurrentDirectory + "/China Geodetic Coordinate System 2000.prj";
                    coorShpPath = System.IO.Path.GetDirectoryName(poiPath) + "/my_mid2_shp.shp";//kb "/my_mid2_shp.shp"是投影的中间文件
                    ISpatialReference spatialRegerence = MyArcEngineMethod.GeometryOperation.CreateSpatialReference(prjPath);
                    coorShpClass = MyArcEngineMethod.FeatureClassOperation.CreateNewFeatureClass(coorShpPath, esriGeometryType.esriGeometryPoint, spatialRegerence);
                }
                catch
                {
                    MessageBox.Show("创建要素类失败，请检查！");
                    return;
                }

                if (coorShpClass == null)
                {
                    MessageBox.Show("创建要素类失败，请检查！");
                    return;
                }
                string fPath1 = @"D:\ArcGis10.2.2\data\projectData\Pshp\timeSpantxt\TimeSpan1.txt";
                if (!File.Exists(fPath1))
                { File.Create(fPath1); }
                StreamWriter swriter1 = new StreamWriter(fPath1);
                string fPath2 = @"D:\ArcGis10.2.2\data\projectData\Pshp\timeSpantxt\TimeSpan2.txt";
                if (!File.Exists(fPath2))
                { File.Create(fPath2); }
                StreamWriter swriter2 = new StreamWriter(fPath2);
                //字段统计
                MyArcEngineMethod.FieldOperation.CreateFieldsByName(coorShpClass, esriFieldType.esriFieldTypeInteger, "Time", "VehiID", "LineID", "PointSeq", "TimeSpan");
                MyArcEngineMethod.FieldOperation.CreateFieldsByName(coorShpClass, esriFieldType.esriFieldTypeDouble, "Lon", "Lat");
                int index_Time = coorShpClass.Fields.FindField("Time");
                int index_VehiID = coorShpClass.Fields.FindField("VehiID");
                int index_LineID = coorShpClass.Fields.FindField("LineID");
                int index_PointSeq = coorShpClass.Fields.FindField("PointSeq");
                int index_TimeSpan = coorShpClass.Fields.FindField("TimeSpan");
                int index_Lon = coorShpClass.Fields.FindField("Lon");
                int index_Lat = coorShpClass.Fields.FindField("Lat");
                //开始创建要素
                IFeatureCursor insertCursor = coorShpClass.Insert(true);//kb 返回可用于插入新feature的光标。
                //车辆记录，线记录，线顺序记录
                TrajectoryPoint lastPoint = new TrajectoryPoint();//kb 车辆ID：-1、点序号：-1、线序号;0
                //进度条// kb参数为：最小值、最大值、进度条值 
                this.ProgressBarSetValue(0, txtContent.Count * 2, 0);//kb ×2是因为第一轮将txt内容逐行遍历到列表中，第二轮是逐个将要素插入更新到要素类中。
                this.label5.Visible = true;
                foreach (var var in txtContent)
                {
                    this.UpdateProgressBar();

                    this.label5.Text = this.progressBar1.Value.ToString() + "/ " + (2 * txtContent.Count).ToString();
                    //当前点
                    TrajectoryPoint curPoint = new TrajectoryPoint();
                    curPoint.PointSeq = lastPoint.PointSeq + 1;//kb lastPoint.PointSeq最初为-1
                    curPoint.LineID = lastPoint.LineID;//kb lastPoint.LineID最开始为0
                    //分割字符串
                    string[] clip = { "," };
                    string[] result = var.Split(clip, StringSplitOptions.RemoveEmptyEntries);//kb 第二个参数表示不保留所返回的空数据元素
                    //字段赋值
                    if (result[0].Length > 8)//kb result[0]是时间
                        result[0] = result[0].Substring(result[0].Length - 8, 8);//kb 取从指定位置开始，具有指定长度的子字符串
                    curPoint.Time = int.Parse(result[0]);//kb 将数字字符串转换为整型=convert.toInt32();
                    if (result[1].Length > 8)
                        result[1] = result[1].Substring(result[1].Length - 8, 8);
                    curPoint.VehiID = int.Parse(result[1]);
                    curPoint.Lon = double.Parse(result[2]);
                    curPoint.Lat = double.Parse(result[3]);
                    #region
                    //判断是不是同一辆车
                    if (curPoint.VehiID != lastPoint.VehiID && (lastPoint.VehiID != -1))
                    {
                        curPoint.LineID = 0;     //kb 应该从1开始编号
                        curPoint.PointSeq = 0;
                    }
                    //判断是不是同一个点 //kb 时间相同的跳过
                    if (curPoint.Time == lastPoint.Time && (lastPoint.VehiID != -1))
                    {
                        //同一个点则跳过
                        continue;
                    }
                    else
                    {
                        //判断是不是需要拆开
                        if ((curPoint.Time - lastPoint.Time) > inter && (lastPoint.VehiID != -1))
                        {
                            curPoint.LineID = lastPoint.LineID + 1;
                            curPoint.PointSeq = 0;//kb 应该从1开始编号
                        }
                    }
                    //判断是不是起始点
                    if (curPoint.PointSeq == 0)//kb 应该判断点序号是否为1
                    {
                        curPoint.TimeSpan = -1;//kb 应该=0
                    }
                    //不是起始点，则计算时间段和速率
                    if (curPoint.TimeSpan != -1)//kb 应该!=0
                    {
                        curPoint.TimeSpan = curPoint.Time - lastPoint.Time;
                    }
                    #endregion
                    #region
                    //if (curPoint.VehiID != lastPoint.VehiID)//kb if车辆ID不同，line的ID归1，点序号归1，时间间隔归0
                    //{
                    //    curPoint.LineID = 1;
                    //    curPoint.PointSeq = 1;
                    //    curPoint.TimeSpan = 0;
                    //}
                    //else
                    //{
                    //    if (curPoint.Time == lastPoint.Time && lastPoint.VehiID != -1)//kb if时间相同，跳过，循环下一个点
                    //    {
                    //        continue;
                    //    }
                    //    else
                    //    {
                    //        if (curPoint.Time - lastPoint.Time > inter && lastPoint.VehiID != -1)//kb if时间间隔太大，line的ID+1，点序号归1，时间间隔归0
                    //        {
                    //            curPoint.LineID += 1;
                    //            curPoint.PointSeq = 1;
                    //            curPoint.TimeSpan = 0;
                    //        }
                    //        else
                    //        {
                    //            if (curPoint.PointSeq == 1)//kb 同一车辆的每个line的第一个点会满足此条件，即不满足：1）车辆ID不同；2）时间相同；3）时间间隔过大
                    //            {
                    //                curPoint.TimeSpan = 0;
                    //            }
                    //            else
                    //            {
                    //                curPoint.TimeSpan = curPoint.Time - lastPoint.Time;
                    //            }
                    //        }
                    //    }
                    //}
                    #endregion
                    lastPoint = curPoint;

                    //生成要素类
                    IFeatureBuffer pBuffer = coorShpClass.CreateFeatureBuffer();//kb 创建可与插入光标一起使用的要素缓冲区
                    IPoint curP = new PointClass();//只是为了下下行给pBuffer的shape赋值
                    curP.PutCoords(curPoint.Lon, curPoint.Lat);
                    pBuffer.Shape = curP;
                    //输入字段属性
                    pBuffer.Value[index_VehiID] = curPoint.VehiID;
                    pBuffer.Value[index_Time] = curPoint.Time;
                    pBuffer.Value[index_LineID] = curPoint.LineID;
                    pBuffer.Value[index_PointSeq] = curPoint.PointSeq;
                    pBuffer.Value[index_TimeSpan] = curPoint.TimeSpan;
                    pBuffer.Value[index_Lon] = curPoint.Lon;
                    pBuffer.Value[index_Lat] = curPoint.Lat;
                    string strTS1 = Convert.ToString(pBuffer.Value[index_TimeSpan]);//kb 第二次时第一点的TimeSpan是0？？？
                    swriter1.Write("\r\n" + strTS1);//kb 文件操作流 写入TimeSpan
                    //
                    insertCursor.InsertFeature(pBuffer);

                }

                insertCursor.Flush();//kb 刷新对数据库的任何未完成缓冲写入
                Marshal.FinalReleaseComObject(insertCursor);

                //将投影坐标系转化为地理坐标系//kb？？？说反了吧
                string prjPath1 = System.Environment.CurrentDirectory + "/CGCS2000 3 Degree GK CM 114E.prj";
                //kb coorShpPath投影到CGCS2000上的中间文件，poiPath原始输出点shp的文件路径
                MyArcEngineMethod.GP_Process.Projection(coorShpPath, poiPath, prjPath1);//kb 投影转换工具。第一个参数：输入要素类路径；第二个参数：输出要素类路径。
                IFeatureClass prjClass = MyArcEngineMethod.FeatureClassOperation.OpenFeatureClass(poiPath);//kb 打开要素类
                //kb 根据字段名为要素类创建字段
                MyArcEngineMethod.FieldOperation.CreateFieldsByName(prjClass, esriFieldType.esriFieldTypeDouble, "X", "Y", "Velocity");//kb 又多创建了"X", "Y", "Velocity"字段
                int new_index_X = prjClass.Fields.FindField("X");
                int new_index_Y = prjClass.Fields.FindField("Y");
                int new_index_Velocity = prjClass.Fields.FindField("Velocity");
                int new_index_VehiID = prjClass.Fields.FindField("VehiID");
                int new_index_TimeSpan = prjClass.Fields.FindField("TimeSpan");
                //kb 返回可用于更新指定查询所选feature的光标
                IFeatureCursor update1 = prjClass.Update(null, false);//kb null：查询条件；false：查询一遍后是否循环
                IFeature mFea = null;
                IFeature lastFea = null;

                //进度条
                this.progressBar1.Maximum = txtContent.Count + prjClass.FeatureCount(null);

                while ((mFea = update1.NextFeature()) != null)
                {

                    this.UpdateProgressBar();
                    this.label5.Text = this.progressBar1.Value.ToString() + "/ " + (txtContent.Count + prjClass.FeatureCount(null)).ToString();

                    IPoint cueP = mFea.ShapeCopy as IPoint;
                    mFea.Value[new_index_X] = cueP.X;
                    mFea.Value[new_index_Y] = cueP.Y;
                    string strTS2 = Convert.ToString(mFea.Value[new_index_TimeSpan]);//kb 第二次执行txt转shp时，第一次循环的strTS2是0？？？
                    swriter2.Write("\r\n"+strTS2);//kb 文件操作流 写入TimeSpan
                    if (mFea.Value[new_index_TimeSpan].ToString() =="-1")//kb 应该起始点的TimeSpan==0
                        mFea.Value[new_index_Velocity] = -1;//kb 应该速度为0
                    else
                        mFea.Value[new_index_Velocity] = MyArcEngineMethod.GeometryOperation.Points2Distance(cueP, lastFea.ShapeCopy as IPoint) / double.Parse(mFea.Value[new_index_TimeSpan].ToString());

                    lastFea = mFea;
                    update1.UpdateFeature(mFea);//kb update1是一个cursor。更新与游标当前位置相对应的数据库中的现有feature。
                }
                update1.Flush();//kb 刷新对数据库的任何未完成缓冲写入
                Marshal.FinalReleaseComObject(update1);
                //删除中间图层
                MyArcEngineMethod.OtherMethod.DeleteSameNameFiles(coorShpPath);

                //结束计时
                sw.Stop();
                TimeSpan sp = sw.Elapsed;
                double minutes = sp.TotalMinutes;
                MessageBox.Show("Txt转点要素成功! 用时：" + minutes + " 分", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.progressBar1.Value = 0;
                swriter1.Close();
                swriter1.Dispose();
                swriter2.Close();
                swriter2.Dispose();
            }


            if (this.mode == 2)
            {
                Stopwatch sw2 = new Stopwatch();
                sw2.Start();

                //
                //判断点要素是否存在
                if (!File.Exists(poiPath))
                {
                    MessageBox.Show("点要素不存在，请检查！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                //判断线要素路径输入是否正确
                if (!linePath.EndsWith(".shp"))
                {
                    MessageBox.Show("线要素路径输入有误，请检查！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                IFeatureClass poiClass = MyArcEngineMethod.FeatureClassOperation.OpenFeatureClass(poiPath);
                int index_poi_VehiID = poiClass.Fields.FindField("VehiID");
                int index_poi_LineID = poiClass.Fields.FindField("LineID");
                int index_poi_TimeSpan = poiClass.Fields.FindField("TimeSpan");

                IFeatureClass lineClass = null;
                ISpatialReference curSR = (poiClass as IGeoDataset).SpatialReference;
                lineClass = MyArcEngineMethod.FeatureClassOperation.CreateNewFeatureClass(linePath, esriGeometryType.esriGeometryPolyline, curSR);
                if (lineClass == null)
                {
                    MessageBox.Show("线要素创建失败，请检查！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                MyArcEngineMethod.FieldOperation.CreateFieldsByName(lineClass, esriFieldType.esriFieldTypeInteger, "VehiID", "LineID");
                int index_line_VehiID = lineClass.Fields.FindField("VehiID");
                int index_line_LineID = lineClass.Fields.FindField("LineID");
                //查询点要素类
                IFeatureCursor search = poiClass.Search(null, false);
                IFeatureCursor insert = lineClass.Insert(true);
                IFeature pFea = null;
                IFeature lastFea = null;
                IPointArray pArray = new PointArrayClass();

                this.ProgressBarSetValue(0, poiClass.FeatureCount(null));
                while ((pFea = search.NextFeature()) != null)
                {
                    this.UpdateProgressBar();

                    string timeSpan = pFea.Value[index_poi_TimeSpan].ToString();
                    if (timeSpan == "-1" || (pFea.OID == poiClass.FeatureCount(null) - 1))
                    {
                        //创建线
                        IPolyline curPl = MyArcEngineMethod.GeometryOperation.GetPolylineByPArray(pArray);
                        
                        if (curPl != null)
                        {
                            IFeatureBuffer pbuff = lineClass.CreateFeatureBuffer();
                            pbuff.Shape = curPl;
                            pbuff.Value[index_line_LineID] = lastFea.Value[index_poi_LineID];
                            pbuff.Value[index_line_VehiID] = lastFea.Value[index_poi_VehiID];
                            insert.InsertFeature(pbuff);
                        }
                        pArray.RemoveAll();
                        pArray.Add(pFea.ShapeCopy as IPoint);
                    }
                    else
                    {
                        pArray.Add(pFea.ShapeCopy as IPoint);
                    }
                    lastFea = pFea;
                }
                insert.Flush();
                ////---------------------------------
                //IFeatureCursor drawCursor = lineClass.Search(null, false);
                //while ((pFea = drawCursor.NextFeature())!=null)
                //{
                //    IPolyline pLine = pFea as IPolyline;
                //    MyArcEngineMethod.ArcMapDrawing.Draw_Polyline()// Draw_Polygon(IActiveView curView, IPolygon polygon, IRgbColor fillcolor, IRgbColor outlineColor, double outlineWidth)
                    
                //}
                ////--------------------------------
                Marshal.FinalReleaseComObject(insert);
                Marshal.FinalReleaseComObject(search);

                sw2.Stop();
                TimeSpan ssp = sw2.Elapsed;
                MessageBox.Show("点要素转线要素成功，用时：" + ssp.Minutes + " 分", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        #region Timer.timer
        private System.Timers.Timer timersTimer = new System.Timers.Timer();
        private void TimerStart()
        {
            timersTimer.Interval = 1000;
            timersTimer.Enabled = true;
            timersTimer.Elapsed += TimersTimer_Elapsed;
            timersTimer.Start();
        }

        private void TimersTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            for (int i = 0; i < 10000; i++)
            {
                this.BeginInvoke(new Action(() =>
                {
                    //  this.label8.Text =  DateTime.UtcNow.ToString();
                }), null);
            }
        }

        private void TimerStop(object sender, EventArgs e)
        {
            timersTimer.Stop();
        }
        #endregion

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.timer1.Stop();
            SendKeys.Send("ENTER");
        }

        #region TextBoxChanged
        private void textBoxChanged()
        {
            if ((this.textBox1.Visible && this.textBox1.Text == "") ||
                (this.textBox2.Visible && this.textBox2.Text == "") ||
                (this.textBox3.Visible && this.textBox3.Text == "") ||
                (this.textBox4.Visible && this.textBox4.Text == ""))
            {
                this.button1.Enabled = false;
                this.button2.Enabled = false;
            }
            else
            {
                this.button1.Enabled = true;
                this.button2.Enabled = true;
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBoxChanged();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBoxChanged();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            textBoxChanged();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            textBoxChanged();
        }
        #endregion

        public void ProgressBarSetValue(int min, int max, int barValue = 0)
        {
            this.progressBar1.Minimum = min;
            this.progressBar1.Maximum = max;
            this.progressBar1.Value = barValue;
        }

        public void UpdateProgressBar()
        {
            this.progressBar1.Value += 1;
            this.progressBar1.Update();
        }
        //kb 浏览并选择文件夹中文件
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog1 = new OpenFileDialog();
            //fileDialog1.InitialDirectory = @"D:\Study\ArcGis\data\wh_didi\Didi20151124\1_50";//kb @是取消转义字符，不用打\\
            fileDialog1.Filter = "文本文件|*.txt|所有文件|*.*";
            //fileDialog1.RestoreDirectory = true;//kb 不记录上次打开的文件目录
            if (fileDialog1.ShowDialog() == DialogResult.OK)//kb 即用户确认选择文件
            { textBox1.Text = System.IO.Path.GetFullPath(fileDialog1.FileName); }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (mode == 1)
            {
                SaveFileDialog fileDialog2 = new SaveFileDialog();
                fileDialog2.Filter = "shp(地图文档)|*.shp|所有文件|*.*";
                if (fileDialog2.ShowDialog() == DialogResult.OK && fileDialog2.FileName.Length > 0)//kb 即用户确认选择文件
                { textBox2.Text = System.IO.Path.GetFullPath(fileDialog2.FileName); }
            }
            if (mode == 2)
            {
                OpenFileDialog fileDialog2 = new OpenFileDialog();
                fileDialog2.Filter = "shp(地图文档)|*.shp|所有文件|*.*";
                if (fileDialog2.ShowDialog() == DialogResult.OK)//kb 即用户确认选择文件
                { textBox2.Text = System.IO.Path.GetFullPath(fileDialog2.FileName); }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SaveFileDialog fileDialog3 = new SaveFileDialog();
            fileDialog3.Filter = "shp(地图文档)|*.shp|所有文件|*.*";
            if (fileDialog3.ShowDialog() == DialogResult.OK && fileDialog3.FileName.Length > 0)//kb 即用户确认选择文件
            { textBox4.Text = System.IO.Path.GetFullPath(fileDialog3.FileName); }
        }

    }
}
