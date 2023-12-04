using Grpc.Core;
using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEditor;
using UnityEngine;

public class LoadYOLO : MonoBehaviour
{
    public NNModel modelAsset;
    private Model m_RuntimeModel;
    private IWorker m_Worker;
    public Texture2D[] inputImage;


    public int imageID = 0;
    void Start()
    {
        m_RuntimeModel = ModelLoader.Load(modelAsset);
        m_Worker = WorkerFactory.CreateWorker( WorkerFactory.Type.Auto , m_RuntimeModel);
    }
    public Texture2D ResizeTexture(Texture2D originalTexture, int width, int height)
    {
        // Create a new empty texture with desired size
        Texture2D resizedTexture = new Texture2D(width, height, originalTexture.format, false);

        // Scale the original texture and copy it to the new texture
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = originalTexture.GetPixelBilinear((float)x / width, (float)y / height);
                resizedTexture.SetPixel(x, y, color);
            }
        }

        // Apply changes to the texture
        resizedTexture.Apply();

        return resizedTexture;
    }
    bool done = false;
    void Update()
    {
        if (!done)
        {
            Tensor input = new Tensor(ResizeTexture(inputImage[imageID], 256, 320), 3);
            m_Worker.Execute(input);
            Tensor O = m_Worker.PeekOutput("output0");
            Debug.Log(O.DataToString());
            //clean memory
            input.Dispose();
            m_Worker.Dispose();
            Resources.UnloadUnusedAssets();
            done = true;
        }
    }

}
