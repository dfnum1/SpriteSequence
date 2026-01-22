/********************************************************************
生成日期:	01:20:2026
类    名: 	SortUpdateJob
作    者:	HappLI
描    述:	Sprite排序Job
*********************************************************************/
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Framework.SpriteSeq
{
    [BurstCompile]
    struct SortUpdateJob : IJob
    {
        public NativeArray<FrameSequenceRenderer.DrawData> drawDatas;
        public NativeHashMap<int, int> guideToIndexs;
        public int count;
        //--------------------------------------------------------
        public void Execute()
        {
            HeapSort(drawDatas, count);
        }
        //--------------------------------------------------------
        void HeapSort(NativeArray<FrameSequenceRenderer.DrawData> arr, int n)
        {
            for (int i = n / 2 - 1; i >= 0; i--)
                Heapify(arr, n, i);
            for (int i = n - 1; i > 0; i--)
            {
                Swap(arr, 0, i);
                Heapify(arr, i, 0);
            }
        }
        //--------------------------------------------------------
        void Heapify(NativeArray<FrameSequenceRenderer.DrawData> arr, int n, int i)
        {
            int largest = i;
            int l = 2 * i + 1;
            int r = 2 * i + 2;

            if (l < n && arr[l].order > arr[largest].order)
                largest = l;
            if (r < n && arr[r].order > arr[largest].order)
                largest = r;

            if (largest != i)
            {
                Swap(arr, i, largest);
                Heapify(arr, n, largest);
            }
        }
        //--------------------------------------------------------
        void Swap(NativeArray<FrameSequenceRenderer.DrawData> arr, int a, int b)
        {
            if (a == b) return;
            FrameSequenceRenderer.DrawData tmp = arr[a];
            arr[a] = arr[b];
            arr[b] = tmp;

            guideToIndexs[arr[a].guid] = a;
            guideToIndexs[arr[b].guid] = b;
        }
    }
}
