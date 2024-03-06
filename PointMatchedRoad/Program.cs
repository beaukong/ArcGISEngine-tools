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
 * ���ܣ�ƥ��·��
 * ���룺1.�켣��·�� 2.·��Ҫ�ص�·�� 
 * ��������ɵĵ�Ҫ���ࣻCSV�ļ�
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
             poiPath = @"F:\OtherPeoplesFile\LiYing\Dissertation\Data\RoadMatch\testdata\PointTest.shp";//��һ���������
            roadPath = @"F:\OtherPeoplesFile\LiYing\Dissertation\Data\RoadMatch\testdata\Road.shp";//�ڶ����������
#endif


#if RELEASE

            poiPath = args[0];//��һ���������
            roadPath = args[1];//�ڶ����������    
            matchedPoiPath = args[2];//��3���������    
#endif

            if ((!File.Exists(poiPath)) ||
                (!File.Exists(roadPath)) )
            {
                Console.WriteLine("�����������·�������ڣ����飡");
                return;
            }

            try
            {
                //ƥ���
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
