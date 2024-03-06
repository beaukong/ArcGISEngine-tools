using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.ArcMapUI;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Editor;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.SystemUI;
using Microsoft.VisualBasic;
using System.IO;

namespace Terrain
{
    class VectorToGrid
    {
        public void VectorToImage()
        {
            Size outRect = new Size(224, 224);
            //等高线shp图层
            string edgeShpPath = @"F:\VSCode\Terrain\data\0datasource\sample\Contour_600mbuf0603.shp";//Contour_600mBuf0223_1_20m100mGen10mDens100m.shp";//Contour_600mbuf0410;
            //edgeShpPath = Interaction.InputBox("等高线shp图层的路径", "等高线shp图层的路径", edgeShpPath, -1, -1);
            if (edgeShpPath == "")
                return;
            if (!File.Exists(edgeShpPath))
            {
                MessageBox.Show("等高线shp图层不存在");
                return;
            }
            IFeatureClass ClsContour = null;
            ClsContour = MyArcEngineMethod.FeatureClassOperation.OpenFeatureClass(edgeShpPath);
            if (ClsContour == null)
            {
                MessageBox.Show("等高线shp图层打开失败");
                return;
            }
            ISpatialReference curSR = (ClsContour as IGeoDataset).SpatialReference;

            //遍历矩形框，裁剪等高线
            //矩形框 shp图层
            string juxingKuangShpPath = @"F:\VSCode\Terrain\data\0datasource\sample\juxingkuangTerrain0603Train.shp";//juxingkuangTerrain;//Contour_600mbuf0410;
            //juxingKuangShpPath = Interaction.InputBox("矩形框 shp图层的路径", "矩形框 shp图层的路径", juxingKuangShpPath, -1, -1);
            if (juxingKuangShpPath == "")
                return;
            if (!File.Exists(juxingKuangShpPath))
            {
                MessageBox.Show("矩形框shp图层不存在");
                return;
            }
            IFeatureClass juxingKuang_Fcls = null;
            juxingKuang_Fcls = MyArcEngineMethod.FeatureClassOperation.OpenFeatureClass(juxingKuangShpPath);
            if (juxingKuang_Fcls == null)
            {
                MessageBox.Show("矩形shp图层打开失败");
                return;
            }
            int index_TagID = juxingKuang_Fcls.FindField("TagID");
            int index_mode = juxingKuang_Fcls.FindField("mode");
            ////创建目录
            //for (int i = 0; i <= 5; i++)
            //{
            //    string path = "F:\\VSCode\\Terrain\\program\\VectorToGrid\\data\\Terrain0603Test\\" + Convert.ToString(i);
            //    if (!Directory.Exists(path))
            //    {
            //        Directory.CreateDirectory(path);
            //    }
            //}
            //区分 训练验证集
            string file_train_graphs_indicator = @"F:\VSCode\Terrain\program\diffpool-master-beforeAddConv\data\Terrain0603Train\Terrain0603Trainsoft-assign_l3x1_batchsize30_LR0.0001_ar0.1_lp_h30_o30weight_decay0.001_label1_epoch_0_TrainAcc.txt";
            string file_val_graphs_indicator = @"F:\VSCode\Terrain\program\diffpool-master-beforeAddConv\data\Terrain0603Train\Terrain0603Trainsoft-assign_l3x1_batchsize30_LR0.0001_ar0.1_lp_h30_o30weight_decay0.001_label1_epoch_0_ValidationAcc.txt";
            StreamReader srTrain = new StreamReader(file_train_graphs_indicator);
            StreamReader srVal = new StreamReader(file_val_graphs_indicator);
            string line = "";
            string[] clip = { " " };
            List<string> tagIDTrain = new List<string>();
            List<string> tagIDVal = new List<string>();
            while ((line = srTrain.ReadLine()) != null)
            {
                string[] lineArr = line.Split(clip, StringSplitOptions.RemoveEmptyEntries);
                tagIDTrain.Add(Convert.ToString(Convert.ToInt32(Convert.ToDouble(lineArr[0]))));
            }
            while ((line = srVal.ReadLine()) != null)
            {
                string[] lineArr = line.Split(clip, StringSplitOptions.RemoveEmptyEntries);
                tagIDVal.Add(Convert.ToString(Convert.ToInt32(Convert.ToDouble(lineArr[0]))));
            }            
            IFeatureCursor cursor_juxingKuang = juxingKuang_Fcls.Search(null, false);
            IFeature feaJuxingKuang = null;
            while ((feaJuxingKuang = cursor_juxingKuang.NextFeature()) != null)
            {
                //空间查询与矩形框Buffer相交的等高线
                ISpatialFilter spaFil = new SpatialFilterClass();
                spaFil.Geometry = feaJuxingKuang.ShapeCopy;
                spaFil.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                IFeatureCursor cursorIntersectContour = ClsContour.Search(spaFil, false);
                IFeature feaIntersectContour = null;
                IGeometryArray lineForCut = new GeometryArrayClass();
                while ((feaIntersectContour = cursorIntersectContour.NextFeature()) != null)
                {
                    lineForCut.Add(feaIntersectContour.ShapeCopy as IPolyline);//获得与当前矩形框相交的等高线
                }
                cursorIntersectContour.Flush();//25   
                Marshal.FinalReleaseComObject(cursorIntersectContour);
                //裁剪与矩形框Buffer相交的等高线，#添加到 新建的等高线shp 不添加了
                IGeometryArray lineForOut = new GeometryArrayClass();
                IGeometryServer2 cutServer = new ESRI.ArcGIS.Geometry.GeometryServerImplClass();
                IGeometryArray lineForPntCut = new GeometryArrayClass();
                IGeometryArray lineResPntCut = new GeometryArrayClass();
                //执行裁剪 Intersect ,得到矩形框Buffer内部的等高线lineForOut
                lineForOut = cutServer.Intersect(curSR, lineForCut, feaJuxingKuang.ShapeCopy);
                if (lineForOut.Count <= 0)
                    continue;
                //将裁剪的要素添加到新建要素类中
                //要创建的各样本的临时线图层
                string pathedgeInGraph_occ = @"F:\VA2012\project\Building\Terrain\临时Line.shp";
                //pathedgeInGraph_occ = Interaction.InputBox("要创建的边shp的路径", "要创建的边shp的路径", pathedgeInGraph_occ, -1, -1);
                if (pathedgeInGraph_occ == "")
                    return;
                IFeatureClass lineClassocc = null;
                lineClassocc = MyArcEngineMethod.FeatureClassOperation.CreateNewFeatureClass(pathedgeInGraph_occ, esriGeometryType.esriGeometryPolyline, curSR);
                IFeatureCursor insertocc = lineClassocc.Insert(true);
                for (int i = 0; i < lineForOut.Count; i++)
                {
                    IFeatureBuffer pbuff = lineClassocc.CreateFeatureBuffer();
                    pbuff.Shape = lineForOut.get_Element(i);
                    insertocc.InsertFeature(pbuff);
                }
                insertocc.Flush();
                Marshal.FinalReleaseComObject(insertocc);
                //将新建的要素类lineClassocc 添加到IMap对象map_new中，并导出图片
                IFeatureLayer featureLayer = new FeatureLayerClass();
                featureLayer.FeatureClass = lineClassocc;

                IMap map_new = new MapClass();
                map_new.AddLayer(featureLayer);

                IGeoFeatureLayer geofeature = featureLayer as IGeoFeatureLayer;
                ISimpleRenderer render = new SimpleRendererClass();//定义渲染器
                ISymbol symbol = new SimpleLineSymbolClass();//定义渲染符号
                ISimpleLineSymbol linesymbol = symbol as ISimpleLineSymbol;
                //linesymbol.Width = 0.4;// 10;// 默认是0.4
                linesymbol.Color = GetRGBColor(255, 255, 255); // 设置线颜色为白色
                render.Symbol = symbol;
                geofeature.Renderer = render as IFeatureRenderer;

                IEnvelope pEnvelop = feaJuxingKuang.ShapeCopy.Envelope;
                Image image = SaveCurrentToImage(map_new, outRect, pEnvelop);
                string mode = Convert.ToString(feaJuxingKuang.get_Value(index_mode));
                string TagID = Convert.ToString(feaJuxingKuang.get_Value(index_TagID));
                if (tagIDTrain.Contains(TagID))
                {
                    image.Save("F:\\VSCode\\Terrain\\program\\GoogleNetTensorflow\\dataset\\Terrain0603Train\\" + mode + "\\" + TagID + ".png");
                }
                else if (tagIDVal.Contains(TagID))
                {
                    image.Save("F:\\VSCode\\Terrain\\program\\GoogleNetTensorflow\\dataset\\Terrain0603Val\\" + mode + "\\" + TagID + ".png");
                }
                else {
                    MessageBox.Show("该tgID不在训练  验证数据集中");
                }
                //image.Save("F:\\VSCode\\Terrain\\program\\GoogleNetTensorflow\\dataset\\Terrain0603Test\\" + mode + "\\" + TagID + ".png");
            }
        }

        // 获取 RGB 颜色对象
        private static IColor GetRGBColor(int red, int green, int blue)
        {
            IRgbColor color = new RgbColorClass();
            color.Red = red;
            color.Green = green;
            color.Blue = blue;
            return color;
        }

        /// <summary>
        /// 将 Map 上指定范围（该范围为规则区域）内的内容输出到 Image，注意，当图片的行数或列数超过10000左右时，出现原因未知的失败
        /// </summary>
        /// <param name="pMap">需转出的 MAP</param>
        /// <param name="outRect">输出的图片大小</param>
        /// <param name="pEnvelope">指定的输出范围（为 Envelope 类型）</param>
        /// <returns>输出的 Image 具体需要保存为什么格式，可通过 Image 对象来实现</returns>
        public static Image SaveCurrentToImage(IMap pMap, Size outRect, IEnvelope pEnvelope)
        {
            // 赋值
            tagRECT rect = new tagRECT();
            rect.left = rect.top = 0;
            rect.right = outRect.Width;
            rect.bottom = outRect.Height;
            try
            {
                // 转换成 activeView，若为 Layout，则将 Layout 转换为 IActiveView
                IActiveView pActiveView = (IActiveView)pMap;
                // 创建图像，为24位色
                Bitmap image = new Bitmap(outRect.Width, outRect.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                using (Graphics g = Graphics.FromImage(image))
                {
                    // 填充背景色（黑色）
                    g.Clear(Color.Black);
                    int dpi = 96; // 设置输出图像的分辨率
                    pActiveView.Output(g.GetHdc().ToInt32(), dpi, ref rect, pEnvelope, null);
                    g.ReleaseHdc();
                }
                return image;
            }
            catch (Exception excp)
            {
                MessageBox.Show(excp.Message + "将当前地图转出出错，原因未知", "出错提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }


        //// <summary>
        ///// 将Map上指定范围（该范围为规则区域）内的内容输出到Image,注意，当图片的行数或列数超过10000左右时，出现原因未知的失败
        ///// <param name="pMap">需转出的MAP</param>
        ///// <param name="outRect">输出的图片大小</param>
        ///// <param name="pEnvelope">指定的输出范围（为Envelope类型）</param>
        ///// <returns>输出的Image 具体需要保存为什么格式，可通过Image对象来实现</returns>
        //public static Image SaveCurrentToImage(IMap pMap, Size outRect, IEnvelope pEnvelope)
        //{
        //    //赋值
        //    tagRECT rect = new tagRECT();
        //    rect.left = rect.top = 0;
        //    rect.right = outRect.Width;
        //    rect.bottom = outRect.Height;
        //    try
        //    {
        //        //转换成activeView，若为ILayout，则将Layout转换为IActiveView
        //        IActiveView pActiveView = (IActiveView)pMap;
        //        // 创建图像,为24位色
        //        Image image = new Bitmap(outRect.Width, outRect.Height); //System.Drawing.Imaging.PixelFormat.Format24bppRgb;
        //        System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(image);
        //        // 填充背景色(白色)
        //        g.FillRectangle(Brushes.Black, 0, 0, outRect.Width, outRect.Height);
        //        int dpi = 2;// (int)(outRect.Width / pEnvelope.Width)
        //        pActiveView.Output(g.GetHdc().ToInt32(), dpi, ref rect, pEnvelope, null);
        //        g.ReleaseHdc();
        //        return image;
        //    }
        //    catch (Exception excp)
        //    {
        //        MessageBox.Show(excp.Message + "将当前地图转出出错，原因未知", "出错提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        return null;
        //    }
        //}

    }
}
