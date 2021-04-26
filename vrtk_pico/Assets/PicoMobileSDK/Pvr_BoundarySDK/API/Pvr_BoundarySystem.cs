using AOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BoundarySystem_Ext
{
    /// <summary>
    /// Boundary System
    /// </summary>
    public class Pvr_BoundarySystem
    {
        private static Pvr_BoundarySystem instance;
        public static Pvr_BoundarySystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Pvr_BoundarySystem();
                }
                return instance;
            }
        }


        public const int CameraImageWidth = 1280;
        public const int CameraImageHeight = 400;

        /// <summary>
        /// Seethrough camera frame ptr
        /// </summary>
        public IntPtr CameraFramePtr = IntPtr.Zero;

        /// <summary>
        /// Open Boundary(must be call before rendering)
        /// </summary>
        public void OpenBoundary()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Pvr_BoundaryAPI.Pvr_StartSDKBoundary();
#endif
        }

        /// <summary>
        /// Open Boundary
        /// Tips:must call before update() for rendering
        /// </summary>
        public void CloseBoundary()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Pvr_BoundaryAPI.Pvr_ShutdownSDKBoundary();
#endif
        }

        /// <summary>
        /// reset 6dof system
        /// </summary>
        public void Reset6Dof()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Pvr_BoundaryAPI.Pvr_ResetVrModebyForce();
#endif
        }

        /// <summary>
        /// Feed controller fixed pose data
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public float[] GetControllerFixedPoseData(int hand)
        {
            var data = new float[7] { 0, 0, 0, 1, 0, 0, 0 };
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Pvr_ControllerManager.controllerlink != null)
            {
                Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(ref data, Pvr_ControllerManager.controllerlink.javaCVClass, "getControllerFixedSensorState", hand);
            }
#endif

            data[2] = -data[2]; // -z
            data[3] = -data[3]; // -w
            return data;
        }
    }

    /// <summary>
    /// Safe Area Algo Module
    /// </summary>
    public class Pvr_SafeAreaAlgoModule
    {
        private static Pvr_SafeAreaAlgoModule instance;
        public static Pvr_SafeAreaAlgoModule Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Pvr_SafeAreaAlgoModule();
                }
                return instance;
            }
        }

        /// <summary>
        /// Extra info for playerarea
        /// </summary>
        public struct ExtraInfo
        {
            public bool overMaxRange;         // For First Line: The range of user drawings is truely very large or Exceptional data occurred
            public bool includeBigGap;        // Distance between two neighbour points is big
            public int bigGapNum;
            public bool centerOut;            // Rect's center out of ClosedCurve
            public bool removeNarrow;          // Flag of remove NarrowArea
            public bool overMaxRange_more;     // For more Line: Over the range 5*5m, that created by the first line
            public int validShrinkArea;         // After Shrinking Safety Area, Set 0 if No valid area Detected , or Set 1
        }

        /// <summary>
        /// Boundary PlayerArea
        /// </summary>
        public struct BoundaryPlayerArea
        {
            public Vector3 lowerleft;           // lower left
            public Vector3 upperleft;           // upper left
            public Vector3 upperRight;          // upper right
            public Vector3 lowerRight;          // lower right
            public Vector3 center;              // center

            public UInt32 width;                   // width
            public UInt32 height;                  // height
            public bool isLegal;                // is playerArea legal

            public ExtraInfo extraInfo;         // extra info for playerarea
        }

        /// <summary>
        /// Callback
        /// </summary>
        /// <param name="playAreaInfo">player area rect info</param>
        /// <param name="boundaryPoints">points of closed region</param>
        /// <param name="unUsedPoints">unused points</param>
        public delegate void BoundarySystemCallback(BoundaryPlayerArea playAreaInfo, List<Vector3> boundaryPoints, List<List<Vector3>> unusedLines);
        private BoundarySystemCallback boundarySystemCallback = null;

        /// <summary>
        /// Shrink Callback
        /// </summary>
        /// <param name="rectPoints">player area rect detail points info</param>
        /// <param name="boundaryPoints">points of closed region</param>
        /// <param name="unUsedPoints">unused points</param>
        public delegate void BoundarySystemCallbackShrink(List<Vector3> rectPoints, List<Vector3> boundaryPoints, List<List<Vector3>> unusedLines);
        private BoundarySystemCallbackShrink boundarySystemCallbackShrink = null;

        /// <summary>
        /// Register BoundarySystem callback
        /// </summary>
        /// <param name="callback"></param>
        public void RegisterCallBack(BoundarySystemCallback callback)
        {
            this.boundarySystemCallback = callback;
        }

        /// <summary>
        /// Register BoundarySystem Shrink callback
        /// </summary>
        /// <param name="callback"></param>
        public void RegisterCallBackShrink(BoundarySystemCallbackShrink callback)
        {
            this.boundarySystemCallbackShrink = callback;
        }

        /// <summary>
        /// Start BoundarySystem Algo
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public bool Start()
        {
            // First:Register callback
            int ret1 = Pvr_SafeAreaAlgoAPI.Pvr_GSASetCallback(_SafeAreaCallback);
            int ret2 = Pvr_SafeAreaAlgoAPI.Pvr_GSASetCallbackShrink(_SafeAreaCallbackShrink);
            if (ret1 != 0 || ret2 != 0)
            {
                Debug.LogError("BoundarySystem register callback failed!");
                return false;
            }

            // Second:Init Boundary System,using 300cm * 300cm
            int ret = Pvr_SafeAreaAlgoAPI.Pvr_GSAInit(300,300);
            if (ret != 0)
            {
                Debug.LogError("BoundarySystem init failed!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Commit new line
        /// </summary>
        /// <param name="isFistLine">init or reset</param>
        /// <param name="points">point set</param>
        /// <returns></returns>
        public bool CommitNewLineData(bool isFistLine, Vector3[] points)
        {
            Pvr_SafeAreaAlgoAPI.GSALineCollection lineCollection = new Pvr_SafeAreaAlgoAPI.GSALineCollection();
            lineCollection.lineCount = 2;

            Pvr_SafeAreaAlgoAPI.GSALine[] lineArray = new Pvr_SafeAreaAlgoAPI.GSALine[2];
            if (isFistLine)
            {
                lineArray[0].pointArray = IntPtr.Zero;
                lineArray[0].pointCount = 0;
            }
            else
            {
                Pvr_SafeAreaAlgoAPI.GSAPoint3i[] tPoint = new Pvr_SafeAreaAlgoAPI.GSAPoint3i[1];
                tPoint[0] = new Pvr_SafeAreaAlgoAPI.GSAPoint3i() { x = 0, y = 0, z = 0 };
                lineArray[0].pointArray = Marshal.UnsafeAddrOfPinnedArrayElement(tPoint, 0); // unsafe
                lineArray[0].pointCount = 1;
            }

            Pvr_SafeAreaAlgoAPI.GSAPoint3i[] pointArray = new Pvr_SafeAreaAlgoAPI.GSAPoint3i[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                Pvr_SafeAreaAlgoAPI.GSAPoint3i newPoint = new Pvr_SafeAreaAlgoAPI.GSAPoint3i();
                newPoint.x = (int)(points[i].x * 1000);
                newPoint.y = (int)(points[i].y * 1000);
                newPoint.z = (int)(points[i].z * 1000);

                pointArray[i] = newPoint;
            }


            lineArray[1].pointArray = Marshal.UnsafeAddrOfPinnedArrayElement(pointArray, 0); // unsafe
            lineArray[1].pointCount = pointArray.Length;

            lineCollection.lineArray = Marshal.UnsafeAddrOfPinnedArrayElement(lineArray, 0);

            IntPtr collectionPtr = Pvr_BoundaryAPI.StructToIntPtr<Pvr_SafeAreaAlgoAPI.GSALineCollection>(lineCollection);

            int ret = Pvr_SafeAreaAlgoAPI.Pvr_GSAUpDateData(collectionPtr);
            Marshal.FreeHGlobal(collectionPtr);

            if (ret != 0)
            {
                Debug.LogError("BoundarySystem commit new line data failed!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Shutdown BoundarySystem Algo
        /// </summary>
        /// <returns></returns>
        public bool Shutdown()
        {
            int ret = Pvr_SafeAreaAlgoAPI.Pvr_GSAShutDown();
            if (ret != 0)
            {
                Debug.LogError("BoundarySystem shutdown failed!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Query distance between boundary and point
        /// </summary>
        /// <param name="point">point</param>
        /// <returns>
        /// Distance > 0.0, Inside the Boundary
        /// Distance = 0.0, On the Boundary
        /// Distance < 0.0, Outside the Boundary
        /// -1 : Error
        /// </returns>
        public double QueryDistanceOfPoint(Vector3 point)
        {
            //point.Set(point.x * 1000, point.y * 1000, point.z * 1000); // m -> mm
            //IntPtr pointPtr = Pvr_BoundaryAPI.StructToIntPtr<Vector3>(point);
            //double distance = Pvr_SafeAreaAlgoAPI.Pvr_GSABoundaryDetector(pointPtr);
            //Marshal.FreeHGlobal(pointPtr);

            Pvr_SafeAreaAlgoAPI.GSAPoint3i point3i = new Pvr_SafeAreaAlgoAPI.GSAPoint3i();
            point3i.x = (int)(point.x * 1000);
            point3i.y = (int)(point.y * 1000);
            point3i.z = (int)(point.z * 1000);

            double distance = Pvr_SafeAreaAlgoAPI.Pvr_GSABoundaryDetector(ref point3i);

            return distance / 1000.0f; // mm -> m
        }

        /// <summary>
        /// Query distance between boundary and point
        /// </summary>
        /// <param name="point">point</param>
        /// <returns>
        /// Distance > 0.0, Inside the Boundary
        /// Distance = 0.0, On the Boundary
        /// Distance < 0.0, Outside the Boundary
        /// -1 : Error
        /// </returns>
        public double QueryDistanceOfPoint(Vector3 point, bool isPlayArea, ref Vector3 closestPoint, ref Vector3 normalDir)
        {
            //point.Set(point.x * 1000, point.y * 1000, point.z * 1000); // m -> mm
            //IntPtr pointPtr = Pvr_BoundaryAPI.StructToIntPtr<Vector3>(point);
            //double distance = Pvr_SafeAreaAlgoAPI.Pvr_GSABoundaryDetector(pointPtr);
            //Marshal.FreeHGlobal(pointPtr);

            Pvr_SafeAreaAlgoAPI.GSAPoint3i point3i = new Pvr_SafeAreaAlgoAPI.GSAPoint3i();
            point3i.x = (int)(point.x * 1000);
            point3i.y = (int)(point.y * 1000);
            point3i.z = (int)(point.z * 1000);


            Pvr_SafeAreaAlgoAPI.GSAPoint3i closestPoint3i = new Pvr_SafeAreaAlgoAPI.GSAPoint3i();
            Pvr_SafeAreaAlgoAPI.GSAPoint3i normalDir3i = new Pvr_SafeAreaAlgoAPI.GSAPoint3i();

            double distance = Pvr_SafeAreaAlgoAPI.Pvr_GSABoundaryDetector2(ref point3i, isPlayArea, ref closestPoint3i, ref normalDir3i);
            closestPoint.x = closestPoint3i.x / 1000.0f;
            closestPoint.y = closestPoint3i.y / 1000.0f;
            closestPoint.z = closestPoint3i.z / 1000.0f;

            normalDir.x = normalDir3i.x / 1000.0f;
            normalDir.y = normalDir3i.y / 1000.0f;
            normalDir.z = normalDir3i.z / 1000.0f;

            return distance / 1000.0f; // mm -> m
        }

        [MonoPInvokeCallback(typeof(Pvr_SafeAreaAlgoAPI.SafeAreaCallback))]
        private static void _SafeAreaCallback(IntPtr lineCollectionPtr, IntPtr safeAreaRectPtr)
        {
            if (lineCollectionPtr == IntPtr.Zero || safeAreaRectPtr == IntPtr.Zero)
            {
                Debug.LogError("BoundarySystem callback is inValid!");
                return;
            }
         
            if (instance.boundarySystemCallback != null)
            {
                List<Vector3> boundaryPoints = new List<Vector3>();
                List<List<Vector3>> unusedLines = new List<List<Vector3>>();

                int byteNum = Marshal.SizeOf(typeof(Pvr_SafeAreaAlgoAPI.GSALine));
                int byteNum2 = Marshal.SizeOf(typeof(Pvr_SafeAreaAlgoAPI.GSAPoint3i));
                // boundary
                Pvr_SafeAreaAlgoAPI.GSALineCollection collection = Pvr_BoundaryAPI.IntPtrToStruct<Pvr_SafeAreaAlgoAPI.GSALineCollection>(lineCollectionPtr);
                IntPtr ptr;

                for (int i = 0; i < collection.lineCount; i++)
                {
                    ptr = new IntPtr(collection.lineArray.ToInt64() + (byteNum * i));
                    Pvr_SafeAreaAlgoAPI.GSALine line = (Pvr_SafeAreaAlgoAPI.GSALine)Marshal.PtrToStructure(ptr, typeof(Pvr_SafeAreaAlgoAPI.GSALine));

                    List<Vector3> newLine = new List<Vector3>();

                    for (int j = 0; j < line.pointCount; j++)
                    {
                        IntPtr tptr = new IntPtr(line.pointArray.ToInt64() + (byteNum2 * j));
                        Pvr_SafeAreaAlgoAPI.GSAPoint3i point = (Pvr_SafeAreaAlgoAPI.GSAPoint3i)Marshal.PtrToStructure(tptr, typeof(Pvr_SafeAreaAlgoAPI.GSAPoint3i));
                        newLine.Add(new Vector3(point.x / 1000f, point.y / 1000f, point.z / 1000f));                       
                    }

                    if (i == 0) // first line for colsed boundary
                    {
                        boundaryPoints = newLine;
                    }
                    else
                    {
                        unusedLines.Add(newLine);
                    }
                }

                // rect
                Pvr_SafeAreaAlgoAPI.GSARect algoResult = Pvr_BoundaryAPI.IntPtrToStruct<Pvr_SafeAreaAlgoAPI.GSARect>(safeAreaRectPtr);
                BoundaryPlayerArea playAreaInfo = new BoundaryPlayerArea();
                playAreaInfo.lowerleft = new Vector3(algoResult.leftup.x / 1000f, algoResult.leftup.y / 1000f, algoResult.leftup.z / 1000f);
                playAreaInfo.upperleft = new Vector3(algoResult.leftdown.x / 1000f, algoResult.leftdown.y / 1000f, algoResult.leftdown.z / 1000f);
                playAreaInfo.upperRight = new Vector3(algoResult.rightdown.x / 1000f, algoResult.rightdown.y / 1000f, algoResult.rightdown.z / 1000f);
                playAreaInfo.lowerRight = new Vector3(algoResult.rightup.x / 1000f, algoResult.rightup.y / 1000f, algoResult.rightup.z / 1000f);
                playAreaInfo.center = new Vector3(algoResult.center.x / 1000f, algoResult.center.y / 1000f, algoResult.center.z / 1000f);
                playAreaInfo.width = algoResult.width;
                playAreaInfo.height = algoResult.height;
                playAreaInfo.isLegal = algoResult.isLegal == 0 ? false : true;

                // extra info
                playAreaInfo.extraInfo.overMaxRange = algoResult.legalData.overMaxRange == 0 ? false : true;
                playAreaInfo.extraInfo.includeBigGap = algoResult.legalData.includeBigGrap == 0 ? false : true;
                playAreaInfo.extraInfo.bigGapNum = algoResult.legalData.bigGapNum;
                playAreaInfo.extraInfo.centerOut = algoResult.legalData.centerOut == 0 ? false : true;
                playAreaInfo.extraInfo.removeNarrow = algoResult.legalData.removeNarrow == 0 ? false : true;
                playAreaInfo.extraInfo.overMaxRange_more = algoResult.legalData.overMaxRange_more == 0 ? false : true;
                playAreaInfo.extraInfo.validShrinkArea = algoResult.legalData.validShrinkArea;

                instance.boundarySystemCallback(playAreaInfo, boundaryPoints, unusedLines);
            }
        }

        [MonoPInvokeCallback(typeof(Pvr_SafeAreaAlgoAPI.SafeAreaCallbackShrink))]
        private static void _SafeAreaCallbackShrink(IntPtr lineCollectionPtr, IntPtr safeAreaDetailRectPtr)
        {
            if (lineCollectionPtr == IntPtr.Zero || safeAreaDetailRectPtr == IntPtr.Zero)
            {
                Debug.LogError("BoundarySystem callback is inValid!");
                return;
            }

            if (instance.boundarySystemCallbackShrink != null)
            {
                List<Vector3> boundaryPoints = new List<Vector3>();
                List<List<Vector3>> unusedLines = new List<List<Vector3>>();

                int byteNum = Marshal.SizeOf(typeof(Pvr_SafeAreaAlgoAPI.GSALine));
                int byteNum2 = Marshal.SizeOf(typeof(Pvr_SafeAreaAlgoAPI.GSAPoint3i));
                // boundary
                Pvr_SafeAreaAlgoAPI.GSALineCollection collection = Pvr_BoundaryAPI.IntPtrToStruct<Pvr_SafeAreaAlgoAPI.GSALineCollection>(lineCollectionPtr);
                IntPtr ptr;

                for (int i = 0; i < collection.lineCount; i++)
                {
                    ptr = new IntPtr(collection.lineArray.ToInt64() + (byteNum * i));
                    Pvr_SafeAreaAlgoAPI.GSALine line = (Pvr_SafeAreaAlgoAPI.GSALine)Marshal.PtrToStructure(ptr, typeof(Pvr_SafeAreaAlgoAPI.GSALine));

                    List<Vector3> newLine = new List<Vector3>();

                    for (int j = 0; j < line.pointCount; j++)
                    {
                        IntPtr tptr = new IntPtr(line.pointArray.ToInt64() + (byteNum2 * j));
                        Pvr_SafeAreaAlgoAPI.GSAPoint3i point = (Pvr_SafeAreaAlgoAPI.GSAPoint3i)Marshal.PtrToStructure(tptr, typeof(Pvr_SafeAreaAlgoAPI.GSAPoint3i));
                        newLine.Add(new Vector3(point.x / 1000f, point.y / 1000f, point.z / 1000f));
                    }

                    if (i == 0) // first line for colsed boundary
                    {
                        boundaryPoints = newLine;
                    }
                    else
                    {
                        unusedLines.Add(newLine);
                    }
                }

                // Rect Detail Points
                List<Vector3> rectPoints = new List<Vector3>();
                Pvr_SafeAreaAlgoAPI.GSALineCollection safeAreaRectLineCollection = Pvr_BoundaryAPI.IntPtrToStruct<Pvr_SafeAreaAlgoAPI.GSALineCollection>(safeAreaDetailRectPtr);
                IntPtr safeAreaRectLinePtr;

                for (int i = 0; i < safeAreaRectLineCollection.lineCount; i++)
                {
                    safeAreaRectLinePtr = new IntPtr(safeAreaRectLineCollection.lineArray.ToInt64() + (byteNum * i));
                    Pvr_SafeAreaAlgoAPI.GSALine line = (Pvr_SafeAreaAlgoAPI.GSALine)Marshal.PtrToStructure(safeAreaRectLinePtr, typeof(Pvr_SafeAreaAlgoAPI.GSALine));

                    for (int j = 0; j < line.pointCount; j++)
                    {
                        IntPtr tptr = new IntPtr(line.pointArray.ToInt64() + (byteNum2 * j));
                        Pvr_SafeAreaAlgoAPI.GSAPoint3i point = (Pvr_SafeAreaAlgoAPI.GSAPoint3i)Marshal.PtrToStructure(tptr, typeof(Pvr_SafeAreaAlgoAPI.GSAPoint3i));
                        rectPoints.Add(new Vector3(point.x / 1000f, point.y / 1000f, point.z / 1000f));
                    }
                }

                instance.boundarySystemCallbackShrink(rectPoints, boundaryPoints, unusedLines);
            }
        }
    }   
}

