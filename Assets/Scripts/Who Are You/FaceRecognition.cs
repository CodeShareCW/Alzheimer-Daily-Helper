using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Demo;
using OpenCvSharp.Face;
using System.IO;

public class FaceRecognition : WebCamScript
{
    public TextAsset faces;
    private CascadeClassifier cascadeface;
    private FaceRecognizer recognizer;
    private string[] names;
    private string[] relationships;

    private readonly Size requiredSize = new Size(500, 550);
    private FaceProcessorLive<WebCamTexture> processor;

   

    private void TrainRecognizer(List<FaceRecord> faceRecord )
    {
        // This one was actually used to train the recognizer. I didn't push much effort and satisfied once it
        // distinguished all detected faces on the sample image, for the real-world application you might want to
        // refer to the following documentation:
        // OpenCV documentation and samples: http://docs.opencv.org/3.0-beta/modules/face/doc/facerec/tutorial/facerec_video_recognition.html
        // Training sets overview: https://www.kairos.com/blog/60-facial-recognition-databases
        // Another OpenCV doc: http://docs.opencv.org/2.4/modules/contrib/doc/facerec/facerec_tutorial.html#face-database

        int id = 0;
        var ids = new List<int>();
        var mats = new List<Mat>();
        var namesList = new List<string>();
        var relationshipList = new List<string>();

        foreach (var fr in faceRecord)
        {
            string name = fr.targetName;
            string rel = fr.targetRelationship;
            namesList.Add(name);
            relationshipList.Add(rel);
            UnityEngine.Debug.LogFormat("{0} = {1}", id, name);
            int index = 1;
            foreach (var f in fr.targetFace)
                {
                
                    var texture = new UnityEngine.Texture2D(16, 16);
                    texture.LoadImage(f); // <--- this one has changed in Unity 2017 API and on that version must be changed

//                        ids.Add(id);

                        // each loaded texture is converted to OpenCV Mat, turned to grayscale (assuming we have RGB source) and resized
                    var mat = OpenCvSharp.Unity.TextureToMat(texture);
                    mat = mat.CvtColor(ColorConversionCodes.BGR2GRAY);


                    OpenCvSharp.Rect[] rawFaces = cascadeface.DetectMultiScale(mat, 1.2, 5);
                    foreach(var face in rawFaces)
                    {
                        mat = new Mat(mat, face);
                           
                        if (requiredSize.Width > 0 && requiredSize.Height > 0)
                            mat = mat.Resize(requiredSize);
                    }
                    ids.Add(id);
                    mats.Add(mat);

                    Texture2D tex = OpenCvSharp.Unity.MatToTexture(mat);
                    byte[] b=tex.EncodeToPNG();
                    //File.WriteAllBytes(Application.dataPath + "/SavedImage/face-"+id+".png", b);
                    index++;
                }
            id++;  
         }
        Debug.Log(mats.Count);
        names = namesList.ToArray();
        relationships = relationshipList.ToArray();

        // train recognizer and save result for the future re-use, while this isn't quite necessary on small training sets, on a bigger set it should
        // give serious performance boost
       

        recognizer.Train(mats, ids);
        recognizer.Save(Application.persistentDataPath + "/trainedFaceData.xml");
}



    protected override void Awake()
    {


        base.Awake();

        FileStorage storageFaces = new FileStorage(faces.text, FileStorage.Mode.Read | FileStorage.Mode.Memory);
        cascadeface = new CascadeClassifier();
        if (!cascadeface.Read(storageFaces.GetFirstTopLevelNode()))
            throw new System.Exception("FaceProcessor.Initialize: Failed to load faces cascade classifier");

        recognizer = FaceRecognizer.CreateLBPHFaceRecognizer();
        TrainRecognizer(AppManager.instance.currentUser.faceRecord);

        string filename = Application.persistentDataPath + "/trainedFaceData.xml";

        recognizer.Load(new FileStorage(File.ReadAllText(filename), FileStorage.Mode.Read | FileStorage.Mode.Memory));



        processor = new FaceProcessorLive<WebCamTexture>();
        processor.Initialize(faces.text,null,null);
        //processor.Initialize(faces.text, eyes.text, shapes.bytes);

        // performance data - some tricks to make it work faster
        processor.Performance.Downscale = 256;          // processed image is pre-scaled down to N px by long side
        processor.Performance.SkipRate = 0;             // we actually process only each Nth frame (and every frame for skipRate = 0)

    }

    protected override void RunRecognizer()
    {
        Mat image = OpenCvSharp.Unity.TextureToMat(renderedtexture);
        var gray = image.CvtColor(ColorConversionCodes.BGR2GRAY);
        OpenCvSharp.Rect[] rawFaces = cascadeface.DetectMultiScale(image, 1.2, 5);

        foreach (var faceRect in rawFaces)
        {
            var grayFace = new Mat(gray, faceRect);
            if (requiredSize.Width > 0 && requiredSize.Height > 0)
                grayFace = grayFace.Resize(requiredSize);

            // now try to recognize the face:
            // "confidence" here is actually a misguide. in fact, it's "distance from the sample to the closest known face" where
            // exact metric is not disclosed in the docs, but checking returned values I found "confidence" to be like 70-100 for
            // positive match with LBPH algo and more like 700-1200 for positive match with EigenFaces/FisherFaces. Unfortunately,
            // all that data isn't much helpful for real life as you don't get adequate % of the confidence, the only thing you
            // actually know is "less is better" with 0 being some "ideal match"
            int label = -1;
            double confidence = 0.0;
            recognizer.Predict(grayFace, out label, out confidence);

            Debug.Log("Prediction: "+confidence);

            bool found = confidence < 85;
            Scalar frameColor = found ? Scalar.LightGreen : Scalar.Red;
            Cv2.Rectangle((InputOutputArray)image, faceRect, frameColor, 4);

            if (confidence < 85)
            {
                int line = 0;
                const int textPadding = 3;
                const double textScale = 1.0;
                string targetName = string.Format("Name: {0}", names[label], (int)confidence);
                string targetRelationship = string.Format("Rel: {0}", relationships[label], (int)confidence);
                var textSize_targetName = Cv2.GetTextSize(targetName, HersheyFonts.HersheyTriplex, textScale, 1, out line);
                var textSize_targetRelationship = Cv2.GetTextSize(targetRelationship, HersheyFonts.HersheyTriplex, textScale, 1, out line);
                var textBox_targetName = new OpenCvSharp.Rect(
                    faceRect.X + (faceRect.Width - textSize_targetName.Width) / 2 - textPadding,
                    faceRect.Bottom,
                    textSize_targetName.Width + textPadding * 2,
                    textSize_targetName.Height + textPadding * 2
                );

                var textBox_targetRelationship = new OpenCvSharp.Rect(
                    faceRect.X + (faceRect.Width - textSize_targetRelationship.Width) / 2 - textPadding,
                    faceRect.Top,
                    textSize_targetRelationship.Width + textPadding * 2,
                    textSize_targetRelationship.Height + textPadding * 2
                );

                Scalar frameColor_targetName = Scalar.LightGreen;
                Scalar frameColor_targetRelationship = Scalar.LightGreen;

                Cv2.Rectangle((InputOutputArray)image, textBox_targetName, frameColor_targetName, -1);
                Cv2.Rectangle((InputOutputArray)image, textBox_targetRelationship, frameColor_targetRelationship, -1);

                confidence = System.Math.Round(confidence, 2);

                image.PutText(targetName, textBox_targetName.TopLeft + new Point(textPadding, textPadding + textSize_targetName.Height), HersheyFonts.HersheyTriplex, textScale, Scalar.Black, 2);
                image.PutText(targetRelationship, textBox_targetRelationship.TopLeft + new Point(textPadding, textPadding + textSize_targetRelationship.Height), HersheyFonts.HersheyTriplex, textScale, Scalar.Black, 2);

            }
        }

        // Render texture
        var texture = OpenCvSharp.Unity.MatToTexture(image);
        var rawImage = gameObject.GetComponent<UnityEngine.UI.RawImage>();
        rawImage.texture = texture;

        var transform = gameObject.GetComponent<UnityEngine.RectTransform>();
        transform.sizeDelta = new UnityEngine.Vector2(image.Width, image.Height);
        
    }



    protected override bool ProcessTexture(WebCamTexture input, ref Texture2D output)
    {
        // detect everything we're interested in
        processor.ProcessTexture(input, TextureParameters);

        //processor.MarkDetected();

        // processor.Image now holds data we'd like to visualize
        output = OpenCvSharp.Unity.MatToTexture(processor.Image, output);   // if output is valid texture it's buffer will be re-used, otherwise it will be re-created

        return true;
    }

    public void ReverseCamera()
    {
        //call switch camera from parent
        SwitchCamera();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            RenderingRecognizingScene();
        else
            StopCamera();
    }


}
