#define CONSOLE
//#define ACCURACY
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using MyArcEngineMethod;
using MyArcEngineMethod.MyExtensionMethod;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PointMatchedRoad
{
    [Guid("edd28c5f-077b-461f-bfda-3858be4abca7")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("PointMatchedRoad.PointMatchRoad")]
    public class PointMatchRoad
    {
        
        //获得建筑物质心在道路上的最近点（街景采样点）
        //1.先将所有的道路融合
        //2.遍历所有的建筑物质心
        //3.计算位置误差，创建DisError字段进行记录
        //输入：建筑物质心路径，路网要要素路径,输出路网上的街景采样点路径
        public static void MatchedPoint2(string poiPath, string roadPath, string matchedPoiPath)
        {
            //日志记录，记得调用close方法
            using (MyArcEngineMethod.MyLogger.MyLogger logger = MyArcEngineMethod.MyLogger.MyLoggerFactory.GetInstance(new StackTrace(new StackFrame(true))))
            {
                IFeatureClass poiClass = MyArcEngineMethod.FeatureClassOperation.OpenFeatureClass(poiPath);
                IFeatureClass roadClass = MyArcEngineMethod.FeatureClassOperation.OpenFeatureClass(roadPath);

                //异常检测
                if (poiClass == null || roadClass == null) throw new ArgumentNullException("错误：输入参数为空！");

                //融合所有的道路网
                IFeatureCursor union = roadClass.Search(null, false);
                IGeometry mergedLines = null;
                IFeature mFea = null;
                while ((mFea = union.NextFeature()) != null)
                {
                    if (mergedLines == null)
                    {
                        mergedLines = mFea.ShapeCopy;
                        continue;
                    }
                    else
                    {
                        mergedLines = (mergedLines as ITopologicalOperator).Union(mFea.ShapeCopy);
                    }
                }
                Marshal.FinalReleaseComObject(union);

#if CONSOLE
                Console.WriteLine("路网已融合为一个要素！");
                int consoleCount = 0; int total = poiClass.FeatureCount(null);
#endif

                //融合完成,创建要素类，然后生成要素类文件
                IFeatureClass generatedPointClass = MyArcEngineMethod.FeatureClassOperation.
        CreateNewFeatureClass(matchedPoiPath, esriGeometryType.esriGeometryPoint, poiClass, true);
                //int errorIndex = MyArcEngineMethod.FieldOperation.CreateOneFieldAndGetIndex(poiClass,
                //  esriFieldType.esriFieldTypeDouble, "DisError");
                int XSamplePointIndex = MyArcEngineMethod.FieldOperation.CreateOneFieldAndGetIndex(poiClass,
    esriFieldType.esriFieldTypeDouble, "XSamPoint");
                int YSamplePointIndex = MyArcEngineMethod.FieldOperation.CreateOneFieldAndGetIndex(poiClass,
esriFieldType.esriFieldTypeDouble, "YSamPoint");
                int headingIndex = MyArcEngineMethod.FieldOperation.CreateOneFieldAndGetIndex(poiClass,
                    esriFieldType.esriFieldTypeDouble, "heading");
                int pitchAngleIndex = MyArcEngineMethod.FieldOperation.CreateOneFieldAndGetIndex(poiClass,
                    esriFieldType.esriFieldTypeDouble, "pitch");
                int floorIndex=poiClass.FindField("FLOOR");
                Console.WriteLine("FLOOR字段索引是" + floorIndex);
                //复制要素类字段
                MyArcEngineMethod.FieldOperation.CopyFeatureClassFields(poiClass, generatedPointClass);
                //int poiTagIDindex = MyArcEngineMethod.FieldOperation.CreateOneFieldAndGetIndex(generatedPointClass,
                //    esriFieldType.esriFieldTypeInteger, "TagID");
                logger.Message("generatedPointShp created successfully!");

                IFeatureCursor poiUpdate = poiClass.Update(null, false);
                IFeatureCursor insert = generatedPointClass.Insert(true);
                //double dis = 0;
                while ((mFea = poiUpdate.NextFeature()) != null)
                {
                    IPoint p = (mergedLines as IProximityOperator).ReturnNearestPoint((mFea.ShapeCopy as IPoint),
                        esriSegmentExtension.esriNoExtension);
                    if (p == null)
                    {
                        continue;
                    }
                    //dis = p.GetDistance(mFea.ShapeCopy as IPoint);
                    //mFea.Value[errorIndex] = dis;
                    IPoint pBuilding = mFea.ShapeCopy as IPoint;
                    mFea.Value[XSamplePointIndex] = p.X;
                    mFea.Value[YSamplePointIndex] = p.Y;
                    double angle=Math.Round(CalLinesAngle(pBuilding.X, pBuilding.Y,p.X, p.Y,  p.X, p.Y+1),0);
                    mFea.Value[headingIndex] = angle;
                    double floor = Convert.ToDouble(mFea.get_Value(floorIndex));
                    double pitchAngle = GetPitchAngle(floor);//GetPitchAngleByEdge(p.X, p.Y, pBuilding.X, pBuilding.Y, floor); //Math.Atan(floor * 2 / dis) / Math.PI * 180;
                    mFea.Value[pitchAngleIndex] = pitchAngle;
                    poiUpdate.UpdateFeature(mFea);

                    //插入元素
                    IFeatureBuffer fb = generatedPointClass.CreateFeatureBuffer();
                    fb.Shape = p;
                    //fb.Value[poiTagIDindex] = mFea.OID;//写入原点要素的OID
                    //复制字段
                    for (int i = 0; i < mFea.Fields.FieldCount; i++)
                    {
                        IField field = mFea.Fields.get_Field(i);
                        if (field.Editable && field.Type != esriFieldType.esriFieldTypeGeometry)
                        {
                            int index1 = fb.Fields.FindField(field.Name);
                            if (index1 != -1)
                            {
                                fb.Value[index1] = mFea.get_Value(i);
                            }
                        }
                    }
                    insert.InsertFeature(fb);
                }
                poiUpdate.Flush();
                Marshal.FinalReleaseComObject(poiUpdate);
                insert.Flush();
                Marshal.FinalReleaseComObject(insert);
                logger.Message("Matched finished!");
            }
        }

        //计算两直线段之间的夹角  pt2X,pt2Y是顶角的坐标，
        internal static double CalLinesAngle(double pt1X, double pt1Y, double pt2X, double pt2Y, double pt3X, double pt3Y)
        {
            if (pt1X == pt2X && pt1Y >= pt2Y)
                return 0;
            if (pt1X == pt2X && pt1Y < pt2Y)
                return 180;
            if (pt1Y == pt2Y && pt1X > pt2X)
                return 90;
            if (pt1Y == pt2Y && pt1X < pt2X)
                return 270;
            double a; double b; double c;
            a = CalculateDistance(pt1X, pt1Y, pt2X, pt2Y);
            b = CalculateDistance(pt3X, pt3Y, pt2X, pt2Y);
            c = CalculateDistance(pt1X, pt1Y, pt3X, pt3Y);
            if (a > 0 && b > 0)
            {
                double degreeValue = 0.0;
                double cosvalue = (a * a + b * b - c * c) / (2 * a * b);
                if ((cosvalue + 1) <= 0)
                    degreeValue = Math.PI;
                else
                    degreeValue = Math.Acos(cosvalue);
                degreeValue = degreeValue * 180 / Math.PI;
                degreeValue = (pt2X < pt1X) ? degreeValue : 360 - degreeValue;

                //degreeValue = pt2X < pt1X ? degreeValue : 180 + degreeValue;

                return degreeValue;
            }
            else // 表示存在重复点情况
            {
                return 0;
            }
        }

        //计算两点之间的欧氏距离
        internal static double CalculateDistance(double pt1X, double pt1Y, double pt2X, double pt2Y)
        {
            return Math.Sqrt(Math.Pow(pt1X - pt2X, 2) + Math.Pow(pt1Y - pt2Y, 2));
        }
        //获得街景采样的俯仰角
        public static double GetPitchAngle(double Floor) {
            if (Floor <= 10)
                return 5;
            else if( Floor <=25)
                return 15;
            else
                return 25;

            //0-10楼 ，pitch=0
            //10-25  10
            //25-    20
            
            //40   20
        }

        
        /// <summary>
        /// //根据3点构成 的3条边长，获得街景采样的俯仰角
        /// </summary>
        /// <param name="x1">匹配点的x坐标</param>
        /// <param name="y1">匹配点的y坐标</param>
        /// <param name="x2">建筑物质心的x坐标</param>
        /// <param name="y2">建筑物质心的y坐标</param>
        /// <param name="x3">建筑物屋顶质心处的x坐标</param>
        /// <param name="y3">建筑物屋顶质心处的x坐标</param>
        /// <returns></returns>
        public static double GetPitchAngleByEdge(double x1,double y1,double x2,double y2,double floor)//,double x3,double y3,double floor)
        {                        
            //double length1 = CalculateDistance(x2, y2, x3, y3);//(x2 - x3) * (x2 - x3) + (y2 - y3) * (y2 - y3);
            //double length2 = CalculateDistance(x1,y1,x3,y3); //(x1 - x3) * (x1 - x3) + (y1 - y3) * (y1 - y3);
            //double length3 = CalculateDistance(x1,y1,x2,y2);// (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
            //double cos1 = (length2 * length2 + length3 * length3 - length1*length1) / (2 * length2 * length3);
            //double degreeValue = Math.Acos(cos1);
            //degreeValue = degreeValue * 180 / Math.PI;
            double tan1 = (floor * 1.5 / CalculateDistance(x1, y1, x2, y2));
            double degreeValue = Math.Atan(tan1);
            degreeValue = degreeValue * 180 / Math.PI;
            return degreeValue;
        }
    }
}
