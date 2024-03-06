using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
    class ToolMethods
    {
        /// <summary>
        /// 利用txt文件生成点要素类的shapefile文件
        /// </summary>
        /// <param name="txtPath">txt路径</param>
        /// <param name="shapefilePath">待生成点要素的路径</param>
        /// <param name="simpleInterval">采样间隔，两点之间相隔时间大于采样间隔，则断开为一条新的轨迹线</param>
        /// <returns></returns>
        public static bool TxtFileToShapefile(string txtPath, string shapefilePath,int simpleInterval)
        {
            //判断txt文件是否存在
            if (!File.Exists(txtPath))
            {
                MessageBox.Show("Txt文件不存在，请检查路径！");
                return false;
            }
            //判断shp路径是否以shp结尾
            if (!shapefilePath.EndsWith(".shp"))
            {
                MessageBox.Show("shp路径输入有误，请检查！");
                return false;
            }
            //解析txt文件
            List<string> txtContent = new System.Collections.Generic.List<string>();
            StreamReader sr = new StreamReader(txtPath);
            string line = "";
            line = sr.ReadLine();
            if (line != "Time,VehiID,Lon,Lat")
            {
                MessageBox.Show("Txt内容有误，读取到的第一行为：{0}",line);
                return false;
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
                string spatialPath = System.Environment.CurrentDirectory + "/China Geodetic Coordinate System 2000.prj";// spatialPath是空间参考文件，
                coorShpPath = System.IO.Path.GetDirectoryName(shapefilePath) + "/my_mid2_shp.shp";
                ISpatialReference spatialRegerence = MyArcEngineMethod.GeometryOperation.CreateSpatialReference(spatialPath);//根据spatialPath创建空间参考。
                coorShpClass = MyArcEngineMethod.FeatureClassOperation.CreateNewFeatureClass(coorShpPath, esriGeometryType.esriGeometryPoint, spatialRegerence);
            }
            catch
            {
                MessageBox.Show("创建要素类失败，请检查！");
                return false;
            }

            if (coorShpClass == null)
            {
                MessageBox.Show("创建要素类失败，请检查！");
                return false;
            }
            
            //字段统计
            MyArcEngineMethod.FieldOperation.CreateFieldsByName(coorShpClass, esriFieldType.esriFieldTypeInteger, "Time", "VehiID", "LineID", "PointSeq","TimeSpan");
            MyArcEngineMethod.FieldOperation.CreateFieldsByName(coorShpClass, esriFieldType.esriFieldTypeDouble, "Lon", "Lat");
            int index_Time = coorShpClass.Fields.FindField("Time");
            int index_VehiID = coorShpClass.Fields.FindField("VehiID");
            int index_LineID = coorShpClass.Fields.FindField("LineID");
            int index_PointSeq = coorShpClass.Fields.FindField("PointSeq");
            int index_TimeSpan = coorShpClass.Fields.FindField("TimeSpan");
            int index_Lon = coorShpClass.Fields.FindField("Lon");
            int index_Lat = coorShpClass.Fields.FindField("Lat");        
            //开始创建要素
            IFeatureCursor insertCursor = coorShpClass.Insert(true);
            //车辆记录，线记录，线顺序记录
            TrajectoryPoint lastPoint = new TrajectoryPoint();             
            foreach (var var in txtContent)
            {                
                //当前点
                TrajectoryPoint curPoint = new TrajectoryPoint();
                curPoint.PointSeq = lastPoint.PointSeq+1;
                curPoint.LineID = lastPoint.LineID;
                //分割字符串
                string[] clip = { ","};
                string[] result = var.Split(clip,StringSplitOptions.RemoveEmptyEntries);    
                //字段赋值
                if (result[0].Length > 8)
                    result[0] = result[0].Substring(result[0].Length - 8, 8);
                curPoint.Time = int.Parse(result[0]);
                if (result[1].Length > 8)
                    result[1] = result[1].Substring(result[1].Length - 8, 8);
                curPoint.VehiID = int.Parse(result[1]);
                curPoint.Lon = double.Parse(result[2]);
                curPoint.Lat = double.Parse(result[3]);
                //判断是不是同一辆车
                if (curPoint.VehiID != lastPoint.VehiID && (lastPoint.VehiID != -1))
                {
                    curPoint.LineID = 0;
                    curPoint.PointSeq = 0;
                }
                //判断是不是同一个点
                if (curPoint.Time == lastPoint.Time && (lastPoint.VehiID != -1))
                {
                    //同一个点则跳过
                    continue;
                }
                else
                {
                    //判断是不是需要拆开
                    if ((curPoint.Time - lastPoint.Time) > simpleInterval && (lastPoint.VehiID != -1))
                    {
                        curPoint.LineID = lastPoint.LineID+1;
                        curPoint.PointSeq = 0;
                    }
                }
                //判断是不是起始点
                if (curPoint.PointSeq == 0)
                {
                    curPoint.TimeSpan = -1;
                }
                //不是起始点，则计算时间段和速率
                if (curPoint.TimeSpan != -1)
                {
                    curPoint.TimeSpan = curPoint.Time - lastPoint.Time;
                }
                lastPoint = curPoint;

                //生成要素类
                IFeatureBuffer pBuffer = coorShpClass.CreateFeatureBuffer();
                IPoint curP =new PointClass();
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
                //
                insertCursor.InsertFeature(pBuffer);

            }

            insertCursor.Flush();
            Marshal.FinalReleaseComObject(insertCursor);

            //将投影坐标系转化为地理坐标系
            string prjPath = System.Environment.CurrentDirectory + "/CGCS2000 3 Degree GK CM 114E.prj";
            MyArcEngineMethod.GP_Process.Projection(coorShpPath,shapefilePath,prjPath);
            IFeatureClass prjClass = MyArcEngineMethod.FeatureClassOperation.OpenFeatureClass(shapefilePath);
            MyArcEngineMethod.FieldOperation.CreateFieldsByName(prjClass, esriFieldType.esriFieldTypeDouble, "X", "Y", "Velocity");
            int new_index_X = prjClass.Fields.FindField("X");
            int new_index_Y = prjClass.Fields.FindField("Y");
            int new_index_Velocity = prjClass.Fields.FindField("Velocity");
            int new_index_VehiID = prjClass.Fields.FindField("VehiID");
            int new_index_TimeSpan = prjClass.Fields.FindField("TimeSpan");

            IFeatureCursor update1 = prjClass.Update(null, false);
            IFeature mFea = null;
            IFeature lastFea = null;
            while ((mFea = update1.NextFeature()) != null)
            {
                IPoint cueP = mFea.ShapeCopy as IPoint;
                mFea.Value[new_index_X] = cueP.X;
                mFea.Value[new_index_Y] = cueP.Y;
                if (mFea.Value[new_index_TimeSpan].ToString() == "-1")
                    mFea.Value[new_index_Velocity] = -1;
                else
                    mFea.Value[new_index_Velocity] = MyArcEngineMethod.GeometryOperation.Points2Distance(cueP,lastFea.ShapeCopy as IPoint)/double.Parse(mFea.Value[new_index_TimeSpan].ToString());

                lastFea = mFea;
                update1.UpdateFeature(mFea);

            }
            update1.Flush();
            Marshal.FinalReleaseComObject(update1);

            return true;
        }

        /// <summary>
        /// 将轨迹点shp转化为轨迹线shp，并记录车辆与轨迹线ID信息
        /// </summary>
        /// <param name="pointShapePath"></param>
        /// <param name="lineShpPath"></param>
        /// <returns></returns>
        public static bool TrajectoryPointShp2LineShp(string pointShapePath,string lineShpPath)
        {
            //判断点要素是否存在
            if(!File.Exists(pointShapePath))
            {
                MessageBox.Show("点要素不存在，请检查！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            //判断线要素路径输入是否正确
            if (!lineShpPath.EndsWith(".shp"))
            {
                MessageBox.Show("线要素路径输入有误，请检查！","警告",MessageBoxButtons.OK,MessageBoxIcon.Error);
                return false;
            }
            IFeatureClass poiClass = MyArcEngineMethod.FeatureClassOperation.OpenFeatureClass(pointShapePath);
            IFeatureClass lineClass = null;
            ISpatialReference curSR = (poiClass as IGeoDataset).SpatialReference;
            lineClass = MyArcEngineMethod.FeatureClassOperation.CreateNewFeatureClass(lineShpPath, esriGeometryType.esriGeometryPolyline, curSR);
            if (lineClass == null)
            {
                MessageBox.Show("线要素创建失败，请检查！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            MyArcEngineMethod.FieldOperation.CreateFieldsByName(lineClass, esriFieldType.esriFieldTypeInteger, "VehiID", "LineID");
            int index_line_VehiID = poiClass.Fields.FindField("VehiID");
            int index_line_LineID = poiClass.Fields.FindField("LineID");
            //查询点要素类
            IFeatureCursor search = null;
            IFeatureCursor insert = lineClass.Insert(true);
            IFeature pFea = null;

            List<string> vehiIDList = new List<string>();
            List<string> lineIDList = new List<string>();
            List<string> pointSeqList = new List<string>();
            vehiIDList = MyArcEngineMethod.FieldOperation.GetFeatureClassUniqueFieldValue(poiClass, "VehiID");
            lineIDList = MyArcEngineMethod.FieldOperation.GetFeatureClassUniqueFieldValue(poiClass, "LineID");
            pointSeqList = MyArcEngineMethod.FieldOperation.GetFeatureClassUniqueFieldValue(poiClass, "PointSeq");
            int index_PointSeq = poiClass.Fields.FindField("PointSeq");
            foreach (var vehicle in vehiIDList)
            {
                IQueryFilter qf = new QueryFilterClass();
                qf.WhereClause = "VehiID=" + vehiIDList;
               //判断是否为0 
                if (poiClass.FeatureCount(qf) == 0)
                    continue;               
                foreach (var lineID in lineIDList)
                {
                    qf.WhereClause = qf.WhereClause + "and LineID=" + lineID;
                    if (poiClass.FeatureCount(qf) < 2)
                        continue;
                    search = poiClass.Search(qf, false);
                    List<IFeature> feaList = new List<IFeature>();
                    //将所有点串成线
                    while ((pFea = search.NextFeature()) != null)
                    {
                        feaList.Add(pFea);
                    }
                    //按照pointSeq排序要素
                    int js =0;
                    int[] index_px = new int[feaList.Count];
                    //依次找到要素并排序
                    while (js < feaList.Count)
                    {
                        for (int i = 0; i < feaList.Count; i++)
                        {
                            if (feaList[i].Value[index_PointSeq].ToString() == js.ToString())
                            {
                                index_px[js] = i;
                                js++;
                                break;
                            }
                        }
                    }
                    IPointArray pArray = new PointArrayClass();
                    foreach (var t in index_px)
                    {
                        IPoint ppp = feaList[t].ShapeCopy as IPoint;
                        pArray.Add(ppp);
                    }
                    IPolyline curPl = MyArcEngineMethod.GeometryOperation.GetPolylineByPArray(pArray);
                    if (curPl != null)
                    {
                        IFeatureBuffer pbuff = lineClass.CreateFeatureBuffer();
                        pbuff.Shape = curPl;
                        pbuff.Value[index_line_LineID] = lineID;
                        pbuff.Value[index_line_VehiID] = vehicle;
                        insert.InsertFeature(pbuff);
                    }
                }
            }

            insert.Flush();
            Marshal.FinalReleaseComObject(insert);
            Marshal.FinalReleaseComObject(search);

            return true; 
        }

        /// <summary>
        /// 将轨迹点shp转化为轨迹线shp，并记录车辆与轨迹线ID信息
        /// </summary>
        /// <param name="pointShapePath"></param>
        /// <param name="lineShpPath"></param>
        /// <returns></returns>
        public static bool TrajectoryPointShp2LineShp(string pointShapePath, string lineShpPath, bool newMode)
        {          
            //
            //判断点要素是否存在
            if (!File.Exists(pointShapePath))
            {
                MessageBox.Show("点要素不存在，请检查！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            //判断线要素路径输入是否正确
            if (!lineShpPath.EndsWith(".shp"))
            {
                MessageBox.Show("线要素路径输入有误，请检查！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            IFeatureClass poiClass = MyArcEngineMethod.FeatureClassOperation.OpenFeatureClass(pointShapePath);
            int index_poi_VehiID = poiClass.Fields.FindField("VehiID");
            int index_poi_LineID = poiClass.Fields.FindField("LineID");
            int index_poi_TimeSpan = poiClass.Fields.FindField("TimeSpan");

            IFeatureClass lineClass = null;
            ISpatialReference curSR = (poiClass as IGeoDataset).SpatialReference;
            lineClass = MyArcEngineMethod.FeatureClassOperation.CreateNewFeatureClass(lineShpPath, esriGeometryType.esriGeometryPolyline, curSR);
            if (lineClass == null)
            {
                MessageBox.Show("线要素创建失败，请检查！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            MyArcEngineMethod.FieldOperation.CreateFieldsByName(lineClass, esriFieldType.esriFieldTypeInteger, "VehiID", "LineID");
            int index_line_VehiID = lineClass.Fields.FindField("VehiID");
            int index_line_LineID = lineClass.Fields.FindField("LineID");
            //查询点要素类
            IFeatureCursor search = poiClass.Search(null,false);
            IFeatureCursor insert = lineClass.Insert(true);
            IFeature pFea = null;
            IFeature lastFea = null;
            IPointArray pArray = new PointArrayClass();

            while ((pFea = search.NextFeature()) != null)
            {
                string timeSpan = pFea.Value[index_poi_TimeSpan].ToString();
                if (timeSpan == "-1"||(pFea.OID == poiClass.FeatureCount(null)-1))
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
            Marshal.FinalReleaseComObject(insert);
            Marshal.FinalReleaseComObject(search);

            return true;
        }


    }


    class TrajectoryPoint
    {
      public string[] AttributeField = new string[] { "Time", "VehiID", "LineID", "PointSeq",
          "TimeSpan", "Lon", "Lat", "X", "Y", "Velocity" }; 
        //属性
        public int VehiID { get; set; }
        public int LineID { get; set; }
        public int PointSeq { get; set; }//kb点顺序
        public int Time { get; set; }
        public int TimeSpan { get; set; }

        public double Lon { get; set; }
        public double Lat { get; set; }
        private double X { get; set; }
        private double Y { get; set; }
        private double Velocity { get; set; }


        public TrajectoryPoint()
        {
            this.VehiID = -1;
            this.LineID = 0;//kb
            this.PointSeq = -1;
            //this.LineID = 1;
            //this.PointSeq = 0;
        }

     /// <summary>
     /// 计算两轨迹点之间的距离
     /// </summary>
     /// <param name="lastP"></param>
     /// <returns></returns>
     public double Distance2Point(TrajectoryPoint lastP)
     {
            double dis = -1;
            double k = (this.X - lastP.X) * (this.X - lastP.X) + (this.Y - lastP.Y)*(this.Y - lastP.Y);
            dis = Math.Sqrt(k);
            return dis;
      }
    }
}
