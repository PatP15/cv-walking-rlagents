using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]


public class SnapshotCamera : MonoBehaviour
{
    Camera snapCam;

    int resWidth = 256;
    int resHeight = 256;

    public float imageFreq = 0.5f;

    bool picTime = false;
    public string dirName = "0";
    // Start is called before the first frame update
    void Awake()
    {
        snapCam = GetComponent<Camera>();
        if (snapCam.targetTexture == null) {
            snapCam.targetTexture = new RenderTexture(resWidth, resHeight, 24);

        }
        else {
            resWidth = snapCam.targetTexture.width;
            resHeight = snapCam.targetTexture.height;
        }
        //snapCam.gameObject.SetActive(false);

    }

    float nextActionTime = 0;
    // Update is called once per frame
    void Update()
    {
        if (nextActionTime > 1/imageFreq)
        {
                
            CallTakeSnapshot();
            nextActionTime = 0;
        }
        nextActionTime += Time.deltaTime;

    }

    public void CallTakeSnapshot()
    {
        picTime = true;
    }

    public void LateUpdate()
    {
        if (picTime)
        {
            Texture2D snapshot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            snapCam.Render();
            RenderTexture.active = snapCam.targetTexture;
            snapshot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            byte[] bytes = snapshot.EncodeToPNG();

            string fileName = Snapshotname();
            System.IO.File.WriteAllBytes(fileName, bytes);
            //Debug.Log("pic!");
            picTime = false;

        }
    }

    string Snapshotname()
    {
        return string.Format("{0}/Snapshots/{1}/snap_{2}_{3}_{4}_{5}.png",
            Application.dataPath,
            dirName,
            resWidth,
            resHeight,
            System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"),
            Random.Range(1f,100f));
    }
}
