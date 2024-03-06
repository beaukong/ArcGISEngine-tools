//#define DEBUG_M   
#define RELEASE
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using System.Diagnostics;
using MyArcEngineMethod;
/*
 * 功能：匹配路网
 * 输入：1.轨迹点路径 2.路网要素的路径 
 * 输出：生成的点要素类；CSV文件
 * */

namespace PointMatchedRoad
{
    class Program
    {
        private static LicenseInitializer m_AOLicenseInitializer = new PointMatchedRoad.LicenseInitializer();

        [STAThread()]
        static void Main(string[] args)
        {
            //ESRI License Initializer generated code.
            m_AOLicenseInitializer.InitializeApplication(new esriLicenseProductCode[] { esriLicenseProductCode.esriLicenseProductCodeAdvanced },
            new esriLicenseExtensionCode[] { });
            //ESRI License Initializer generated code.
            args[0] = @"data\Poi.shp";
            args[1] = @"data\road.shp";
            args[2] = @"data\matchedPoi.shp";
            String poiPath;
            String roadPath;
            String matchedPoiPath;
#if DEBUG_M    
             poiPath = @"F:\OtherPeoplesFile\LiYing\Dissertation\Data\RoadMatch\testdata\PointTest.shp";//第一个输入参数
            roadPath = @"F:\OtherPeoplesFile\LiYing\Dissertation\Data\RoadMatch\testdata\Road.shp";//第二个输入参数
#endif


#if RELEASE

            poiPath = args[0];//第一个输入参数
            roadPath = args[1];//第二个输入参数    
            matchedPoiPath = args[2];//第3个输入参数    
#endif

            if ((!File.Exists(poiPath)) ||
                (!File.Exists(roadPath)) )
            {
                Console.WriteLine("错误：输入参数路径不存在！请检查！");
                return;
            }

            try
            {
                //匹配点
                PointMatchRoad.MatchedPoint2(poiPath, roadPath, matchedPoiPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            
            //Do not make any call to ArcObjects after ShutDownApplication()
            m_AOLicenseInitializer.ShutdownApplication();
        }

    }
}
