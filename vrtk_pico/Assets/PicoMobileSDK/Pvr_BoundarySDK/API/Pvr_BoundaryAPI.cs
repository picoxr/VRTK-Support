using System;
using System.Runtime.InteropServices;

namespace BoundarySystem_Ext
{
    /// <summary>
    /// Boundary Module API
    /// </summary>
    internal static class Pvr_BoundaryAPI
    {
        private const string LibFileName = "Pvr_UnitySDK";

        #region DLL API Interface Declaration
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Pvr_StartSDKBoundary();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Pvr_ShutdownSDKBoundary();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int Pvr_GetMainSensorStateExt(ref float x, ref float y, ref float z, ref float w, ref float px, ref float py, ref float pz, ref float vfov, ref float hfov, ref int viewNumber);

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Pvr_GetCameraData_Ext();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void Pvr_ResetVrModebyForce();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool Pvr_ResumeBoundaryForSTS();

        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void Pvr_PauseBoundaryForSTS();
        #endregion

        #region Public DLL API Interface Wrap
        /// <summary>
        /// Get Sensor Pose
        /// </summary>
        public static int UPvr_GetMainSensorStateExt(ref float x, ref float y, ref float z, ref float w, ref float px, ref float py, ref float pz, ref float vfov, ref float hfov, ref int viewNumber)
        {

#if UNITY_ANDROID && !UNITY_EDITOR
            return Pvr_GetMainSensorStateExt(ref x, ref y, ref z, ref w, ref px, ref py, ref pz, ref vfov, ref hfov, ref viewNumber);
#else
            return 0;
#endif
        }

        /// <summary>
        /// Get Camera Frame Data
        /// </summary>
        /// <returns></returns>
        public static IntPtr UPvr_GetCameraData_Ext()
        {
            IntPtr ptr = IntPtr.Zero;
#if UNITY_ANDROID && !UNITY_EDITOR
            ptr = Pvr_GetCameraData_Ext();
#endif
            return ptr;
        }

        /// <summary>
        /// BoundaryResume
        /// </summary>
        /// <returns></returns>
        public static bool UPvr_ResumeBoundaryForSTS()
        {
            bool ret = false;
#if UNITY_ANDROID && !UNITY_EDITOR
            ret = Pvr_ResumeBoundaryForSTS();
#endif
            return ret;
        }

        /// <summary>
        /// BoundaryPause
        /// </summary>
        /// <returns></returns>
        public static void UPvr_PauseBoundaryForSTS()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Pvr_PauseBoundaryForSTS();
#endif
        }

        #endregion

        #region Tools
        /// <summary>
        /// C# Struct to IntPtr
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        public static IntPtr StructToIntPtr<T>(T info)
        {
            int size = Marshal.SizeOf(info);
            IntPtr intPtr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(info, intPtr, true);
            return intPtr;
        }

        /// <summary>
        /// IntPtr to C# Struct
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        /// <returns></returns>
        public static T IntPtrToStruct<T>(IntPtr info)
        {
            return (T)Marshal.PtrToStructure(info, typeof(T));
        }
        #endregion
    }

    /// <summary>
    /// Safety Player Module API
    /// </summary>
    internal static class Pvr_SafeAreaAlgoAPI
    {
        private const string LibFileName = "SafetyArea";
        /// <summary>
        /// Original Guardian Boundary
        /// </summary>
        /// <param name="lineCollectionPtr"></param>
        /// <param name="safeAreaRectPtr"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SafeAreaCallback(IntPtr lineCollectionPtr, IntPtr safeAreaRectPtr);

        /// <summary>
        /// Shrink Guardian Boundary
        /// </summary>
        /// <param name="lineCollectionPtr"></param>
        /// <param name="safeAreaDetailRectPtr"></param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void SafeAreaCallbackShrink(IntPtr lineCollectionPtr, IntPtr safeAreaDetailRectPtr);

        #region Algo Struct
        /// <summary>
        /// Point: single point coordinate（x,y,z）
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct GSAPoint3i
        {
            public int x;
            public int y;
            public int z;
        }

        /// <summary>
        /// Line: set of point
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct GSALine
        {
            public IntPtr pointArray;  // array: every element is a GSAPoint3i, storaged in chronological order
            public int pointCount;     // array size
        }

        /// <summary>
        /// LineCollection: set of line
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct GSALineCollection
        {
            public IntPtr lineArray;    // array，every element is a GSALine
            public int lineCount;       // array size
        }

        /// <summary>
        /// Illegal Data Flag, for single line
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct GSALegalData
        {
            public int overMaxRange;        // For First Line: 1,The range of user drawings is truely very large;2, Exceptional data occurred
            public int includeBigGrap;      // Distance between two neighbour points is big
            public int bigGapNum;           // num of GapNum
            public int centerOut;           // Rect's center out of Closed
            public int removeNarrow;        // Flag of remove NarrowArea
            public int overMaxRange_more;   // For more Line: Over the range 5*5m, that created by the first line
            public int validShrinkArea;     // After Shrinking Safety Area, Set 0 if No valid area Detected , or Set 1
        }

        /// <summary>
        /// Rect: Max Inserted Rectangle(4 vertex, center, width, height, bIfArea2x2)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct GSARect
        {
            public GSAPoint3i leftup;
            public GSAPoint3i leftdown;
            public GSAPoint3i rightup;
            public GSAPoint3i rightdown;
            public GSAPoint3i center;
            public UInt32 width;
            public UInt32 height;
            public int isLegal;
            public GSALegalData legalData;
        }
        #endregion

        #region DLL API Interface Declaration
        /// <summary>
        /// init
        /// </summary>
        /// <returns>0: OK</returns>
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GSAInit(int width,int height);
        /// <summary>
        /// ShutDown
        /// </summary>
        /// <returns>0: OK</returns>
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GSAShutDown();
        /// <summary>
        /// sdk send function ptr to ALG , ALG send data to sdk
        /// </summary>
        /// <param name="cb">0: OK</param>
        /// <returns></returns>
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GSASetCallback(SafeAreaCallback cb);
        /// <summary>
        /// sdk send function ptr to ALG , ALG send data(after Shrink) to sdk
        /// </summary>
        /// <param name="cb">0: OK</param>
        /// <returns></returns>
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GSASetCallbackShrink(SafeAreaCallbackShrink cb);
        /// <summary>
        /// input data: boundary of SafetyArea now（NULL when first or reset）,remaining line data
        /// </summary>
        /// <param name="lineCollectionPtr"></param>
        /// <returns>0: OK</returns>
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GSAUpDateData(IntPtr lineCollectionPtr);
        /// <summary>
        /// Set config file path, defult "/sdcard"
        /// </summary>
        /// <param name="configPath"></param>
        /// <returns>0: OK</returns>
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GSASetConfigFilePath(IntPtr configPath);
        /// <summary>
        /// is SafetyArea update
        /// </summary>
        /// <returns>
        ///     true : Closed Area Update
        ///     false: Closed Area Not Update
        /// </returns>
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool GSAGetClosedAreaUpdateFlag();
        /// <summary>
        /// load boundary to algo
        /// </summary>
        /// <param name="lineCollectionPtr">SafetyArea's Boundary list</param>
        /// <returns>
        /// 0 : ok
        /// -1: failed
        /// </returns>
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GSALoadSafetyArea(IntPtr lineCollectionPtr);
        /// <summary>
        /// Cal Distance of Head_6Dof(or Handle) and SafetyArea's Boundary
        /// </summary>
        /// <param name="pointPtr">Head_6Dof(or Handle) 3D Coordinate</param>
        /// <returns>
        /// Distance between Head_6Dof(or Handle) and SafetyArea's Boundary
        /// Distance > 0.0, Inside the Boundary
        /// Distance = 0.0, On the Boundary
        /// Distance < 0.0, Outside the Boundary
        /// -1 : Error
        /// </returns>
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern double GSABoundaryDetector(ref GSAPoint3i pointPtr);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineCollectionPtr">Boundary list</param>
        /// <param name="isPlayArea">true: PlayArea Rect; false: OutBoundary</param>
        /// <returns> 0 : ok; -1: failed </returns>
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GSALoadSafetyArea2(IntPtr lineCollectionPtr, bool isPlayArea);

        /// <summary>
        /// (1)rerurn Distance of Head_6Dof(or Handle) and SafetyArea's Boundary
	    //  (2)return ClosestPoint
        //  (3)return ClosestPointNormal
        /// <param name="point">Head_6Dof(or Handle) 3D Coordinate</param>
        /// <param name="isPlayArea">true: PlayArea Rect; false: OutBoundary</param>
        /// <param point="closestPoint">true: PlayArea Rect; false: OutBoundary</param>
        /// <param normal="normalDir">true: PlayArea Rect; false: OutBoundary</param>
        /// <returns>
        /// Distance :between Head_6Dof(or Handle) and SafetyArea's Boundary
        ///     *Distance > 0.0, Inside the Boundary
        ///     *Distance = 0.0, On the Boundary
        ///     *Distance < 0.0, Outside the Boundary
        /// </returns>
        [DllImport(LibFileName, CallingConvention = CallingConvention.Cdecl)]
        private static extern double GSABoundaryDetector2(ref GSAPoint3i point, bool isPlayArea, ref GSAPoint3i closestPoint, ref GSAPoint3i normalDir);

        #endregion

        #region Public DLL API Interface Wrap
        public static int Pvr_GSAInit(int width,int height)
        {
            int ret = -1;
#if UNITY_ANDROID && !UNITY_EDITOR
            ret = GSAInit(width,height);
#endif
            return ret;
        }

        public static int Pvr_GSASetCallback(SafeAreaCallback cb)
        {
            int ret = -1;
#if UNITY_ANDROID && !UNITY_EDITOR
            ret = GSASetCallback(cb);
#endif
            return ret;
        }


        public static int Pvr_GSASetCallbackShrink(SafeAreaCallbackShrink cb)
        {
            int ret = -1;
#if UNITY_ANDROID && !UNITY_EDITOR
            ret = GSASetCallbackShrink(cb);
#endif
            return ret;
        }

        public static int Pvr_GSAUpDateData(IntPtr lineCollectionPtr)
        {
            int ret = -1;
#if UNITY_ANDROID && !UNITY_EDITOR
            ret = GSAUpDateData(lineCollectionPtr);
#endif
            return ret;
        }

        public static bool Pvr_GSAGetClosedAreaUpdateFlag()
        {
            bool ret = false;
#if UNITY_ANDROID && !UNITY_EDITOR
            ret = GSAGetClosedAreaUpdateFlag();
#endif
            return ret;
        }

        public static int Pvr_GSASetConfigFilePath(string configPath)
        {
            int ret = -1;
#if UNITY_ANDROID && !UNITY_EDITOR
            IntPtr pathPtr = Marshal.StringToHGlobalAnsi(configPath);
            ret = GSASetConfigFilePath(pathPtr);

            Marshal.FreeHGlobal(pathPtr);
#endif
            return ret;
        }

        public static int Pvr_GSALoadSafetyArea(IntPtr lineCollectionPtr)
        {
            int ret = -1;
#if UNITY_ANDROID && !UNITY_EDITOR
            ret = GSALoadSafetyArea(lineCollectionPtr);
#endif
            return ret;
        }

        public static double Pvr_GSABoundaryDetector(ref GSAPoint3i point)
        {
            double distance = 0;
#if UNITY_ANDROID && !UNITY_EDITOR
            distance = GSABoundaryDetector(ref point);
#endif
            return distance;
        }

        public static int Pvr_GSAShutDown()
        {
            int ret = -1;
#if UNITY_ANDROID && !UNITY_EDITOR
            ret = GSAShutDown();
#endif
            return ret;
        }

        public static int Pvr_GSALoadSafetyArea2(IntPtr lineCollectionPtr, bool isPlayArea)
        {
            int ret = -1;
#if UNITY_ANDROID && !UNITY_EDITOR
            ret = GSALoadSafetyArea2(lineCollectionPtr, isPlayArea);
#endif
            return ret;
        }

        public static double Pvr_GSABoundaryDetector2(ref GSAPoint3i point, bool isPlayArea, ref GSAPoint3i closestPoint, ref GSAPoint3i normalDir)
        {
            double distance = 0;
#if UNITY_ANDROID && !UNITY_EDITOR
            distance = GSABoundaryDetector2(ref point, isPlayArea, ref closestPoint, ref normalDir);
#endif
            return distance;
        }
        #endregion
    }
}

