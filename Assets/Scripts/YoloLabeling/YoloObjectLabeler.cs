using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Barracuda;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using YoloHolo.Services;
using YoloHolo.Utilities;




namespace YoloHolo.YoloLabeling
{
    [RequireComponent(typeof(Camera))]
    public class YoloObjectLabeler : MonoBehaviour
    {
        [SerializeField] private NNModel NNmodel;


        [SerializeField]
        private float minimumProbability = 0.2f;


        [SerializeField]
        private float overlapThreshold = 0.3f;
        public float OverlapThreshold => overlapThreshold;

        [SerializeField]
        private int channels = 3;


        [SerializeField]
        private YoloVersion version = YoloVersion.V8;


        [SerializeField]
        private int cameraFPS = 4;

        [SerializeField]
        private Vector2Int actualCameraSize;

        [SerializeField]
        private Vector2Int yoloImageSize = new(320, 256);

        [SerializeField]
        private float virtualProjectionPlaneWidth = 1.356f;

        [SerializeField]
        private float minIdenticalLabelDistance = 0.3f;

        [SerializeField]
        private float labelNotSeenTimeOut = 5f;

        private Camera snapCam;


        private List<GameObject> currentBoundingBoxes = new List<GameObject>();
        private IWorker worker;

        public Canvas canvas;
        private Dictionary<string, Color> objectColors = new Dictionary<string, Color>();
        private void Start()
        {
            // Load the YOLOv7 model from the provided NNModel asset
            var model = ModelLoader.Load(NNmodel);
            objectColors.Add("obstacle", Color.red);
            objectColors.Add("wall", Color.blue);
            objectColors.Add("target", Color.green);

            // Create a Barracuda worker to run the model on the GPU
            worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
            snapCam = GetComponent<Camera>();
            if (snapCam.targetTexture == null)
            {
                snapCam.targetTexture = new RenderTexture(yoloImageSize.x, yoloImageSize.y, channels);

            }
            else
            {
                yoloImageSize.x = snapCam.targetTexture.width;
                yoloImageSize.y = snapCam.targetTexture.height;
            }

            StartRecognizingAsync();
        }
        /*
        private void Update()
        {
            Time.timeScale = 0.5f;
            Time.fixedDeltaTime = 0.7f;
        }*/

        public List<YoloItem> foundObjects = new List<YoloItem>();
        private async Task StartRecognizingAsync()
        {
            //Debug.Log("Starting recognizing");
            await Task.Delay(1000);
            IYoloClassTranslator translator = new ObstacleTranslator();
            while (true)
            {
                //Debug.Log("Recognizing");
                snapCam.Render();
                await Task.Delay(32);

                //Debug.Log("Rendering");
                var texture = snapCam.targetTexture.ToTexture2D();
                //await Task.Delay(32);

                //Debug.Log("Recognizing objects");
                foundObjects = await RecognizeObjects(texture, translator);
                //Debug.Log("Len of found objects: " + foundObjects.Count);
                for (var index = 0; index < foundObjects.Count; index++)
                {
                    var obj = foundObjects[index];
                    //Debug.Log($"Found: {index} {obj.MostLikelyObject} : {obj.Confidence}");
                }
                //
                //ShowRecognitions(foundObjects, cameraTransform);
                //ShowBoundingBoxes(foundObjects);
                Destroy(texture);
               
                //Destroy(cameraTransform.gameObject);
            }
        }

        private Rect TransformToCameraSpace(YoloItem item)
        {
            // Convert YoloItem coordinates to camera space here
            // This conversion will depend on how your Yolo model represents coordinates
            // For example, you might need to scale from YoloImageSize to actualCameraSize

            Vector2 topLeft = item.TopLeft;
            Vector2 size = item.Size;

            return new Rect(topLeft, size);
        }
        private async Task ShowBoundingBoxes(List<YoloItem> items)
        {
            //Debug.Log("Showing bounding boxes");
            //get all the children of the canvas

            int childrenCount  = canvas.gameObject.transform.childCount;
            for (int i = 0; i < childrenCount; i++)
            {
                Destroy(canvas.gameObject.transform.GetChild(i).gameObject);
            }

            foreach (var item in items)
            {
                // Transform YoloItem coordinates to camera space
                Rect cameraSpaceRect = TransformToCameraSpace(item);

                // Create and position a UI element to represent the bounding box
                GameObject bbox = new GameObject("BoundingBox");
                bbox.transform.SetParent(canvas.transform, false);
                var rectTransform = bbox.AddComponent<RectTransform>();
                rectTransform.anchoredPosition = cameraSpaceRect.position;
                rectTransform.sizeDelta = cameraSpaceRect.size;

                // Assign a color based on the object type
                var image = bbox.AddComponent<Image>();
                if (objectColors.TryGetValue(item.MostLikelyObject, out Color color))
                {
                    image.color = new Color(color.r, color.g, color.b, 0.4f); // Semi-transparent
                }
                else
                {
                    image.color = Color.magenta; // Default color for unrecognized objects
                }

                // Create and position a UI Text element for the object name
                GameObject textObj = new GameObject("Label");
                textObj.transform.SetParent(bbox.transform, false);
                var text = textObj.AddComponent<Text>();
                text.text = item.MostLikelyObject;
                text.alignment = TextAnchor.LowerCenter;
                text.color = Color.white;
                text.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // Assign a font
                text.fontSize = 14;

                var textRectTransform = text.GetComponent<RectTransform>();
                textRectTransform.sizeDelta = new Vector2(100, 30); // Adjust size as needed
                textRectTransform.anchoredPosition = new Vector2(0, -15); // Position below the bbox

               
            }
        }

        private void ClearBoundingBoxes()
        {
            for(int i = 0; i < currentBoundingBoxes.Count; i++)
            {
                if (currentBoundingBoxes[i] != null)
                {
                    Destroy(currentBoundingBoxes[i]);
                }
            }
            //currentBoundingBoxes.Clear();
            /*
            foreach (var bbox in currentBoundingBoxes)
            {
                if (bbox != null)
                {
                    Destroy(bbox); // Destroy the GameObject
                }
            }
            currentBoundingBoxes.Clear(); // Clear the list
            */

        }
        public async Task<List<YoloItem>> RecognizeObjects(Texture2D texture, IYoloClassTranslator translator)
        {
            var inputTensor = new Tensor(texture, channels: channels);
            //Debug.Log("Tensor created");
            //Debug.Log("inputTensor: " + inputTensor.ToString());
            //await Task.Delay(32);
            // Run the model on the input tensor
            try
            {
                worker.Execute(inputTensor);
            }
            catch (Exception e)
            {
                Debug.LogError("Error during model execution: " + e.Message);
            }
            //Debug.Log("Model executed");
            var outputTensor = await ForwardAsync(worker, inputTensor);
            //Debug.Log("Ran model: " + outputTensor.ToString());
            inputTensor.Dispose();

            //Debug.Log("translating");
            var yoloItems = outputTensor.GetYoloData(translator,
                minimumProbability, overlapThreshold, YoloVersion.V8);

            outputTensor.Dispose();
            return yoloItems;
        }
        public async Task<Tensor> ForwardAsync(IWorker modelWorker, Tensor inputs)
        {
            var executor = worker.StartManualSchedule(inputs);
            var it = 0;
            bool hasMoreWork;
            do
            {
                hasMoreWork = executor.MoveNext();
                if (++it % 20 == 0)
                {
                    worker.FlushSchedule();
                    //await Task.Delay(32);
                }
            } while (hasMoreWork);

            return modelWorker.PeekOutput();
        }

        /// <inheritdoc />
        public void Destroy()
        {
            // Dispose of the Barracuda worker when it is no longer needed
            worker?.Dispose();
        }
    }





    
}
