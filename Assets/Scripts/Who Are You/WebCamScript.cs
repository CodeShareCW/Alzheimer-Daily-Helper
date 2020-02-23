using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
public abstract class WebCamScript : MonoBehaviour
{
    [SerializeField] public GameObject displaySurface;
    private Nullable<WebCamDevice> webcamdevice;
    protected WebCamTexture webcamtexture;
    protected Texture2D renderedtexture;
    protected OpenCvSharp.Unity.TextureConversionParams TextureParameters { get; private set; }



    private int currentCameraIndex;

    public string DeviceName
    {
        get
        {
            return (webcamdevice != null) ? webcamdevice.Value.name : null;
        }
        set
        {
            if (value == DeviceName)
                return;
            if (null != webcamtexture && webcamtexture.isPlaying)
                webcamtexture.Stop();
            int cameraIndex = -1;
            for (int i = 0; i < WebCamTexture.devices.Length && -1 == cameraIndex; i++)
            {
                if (WebCamTexture.devices[i].name == value)
                    cameraIndex = i;
            }

            // set device up
            if (-1 != cameraIndex)
            {
                webcamdevice = WebCamTexture.devices[cameraIndex];
                webcamtexture = new WebCamTexture(webcamdevice.Value.name);

                // read device params and make conversion map
                ReadTextureConversionParameters();

                webcamtexture.Play();
            }
            else
            {
                throw new ArgumentException(String.Format("{0}: provided DeviceName is not correct device identifier", this.GetType().Name));
            }
        }
    }

    private void ReadTextureConversionParameters()
    {
       OpenCvSharp.Unity.TextureConversionParams parameters = new OpenCvSharp.Unity.TextureConversionParams();

        // frontal camera - we must flip around Y axis to make it mirror-like
        parameters.FlipHorizontally = webcamdevice.Value.isFrontFacing;

        // TODO:
        // actually, code below should work, however, on our devices tests every device except iPad
        // returned "false", iPad said "true" but the texture wasn't actually flipped

        // compensate vertical flip
        //parameters.FlipVertically = webCamTexture.videoVerticallyMirrored;

        // deal with rotation
        if (0 != webcamtexture.videoRotationAngle)
            parameters.RotationAngle = webcamtexture.videoRotationAngle; // cw -> ccw

        // apply
        TextureParameters = parameters;

        //UnityEngine.Debug.Log (string.Format("front = {0}, vertMirrored = {1}, angle = {2}", webCamDevice.isFrontFacing, webCamTexture.videoVerticallyMirrored, webCamTexture.videoRotationAngle));

    }

    public void StopCamera()
    {
        if (webcamtexture != null && webcamtexture.isPlaying)
        {
            webcamtexture.Stop();
        }
        
    }
    public void StartCamera()
    {
        if (webcamtexture != null && !webcamtexture.isPlaying)
            webcamtexture.Play();
    }
    public void SwitchCamera()
    {
        if (WebCamTexture.devices.Length > 0)
        {
            currentCameraIndex = (currentCameraIndex + 1) % WebCamTexture.devices.Length;
            DeviceName = WebCamTexture.devices[currentCameraIndex].name;
        }
        Debug.Log(currentCameraIndex);
   }


    // Start is called before the first frame update
    protected virtual void Awake()
    {
        currentCameraIndex = 0;
        if (WebCamTexture.devices.Length > 0)
            DeviceName = WebCamTexture.devices[0].name;
     
    }

    // Update is called once per frame
    protected void RenderingRecognizingScene()
    {
        if (webcamtexture != null && webcamtexture.didUpdateThisFrame)
        {
            
            // this must be called continuously
            ReadTextureConversionParameters();

            // process texture with whatever method sub-class might have in mind
            if (ProcessTexture(webcamtexture, ref renderedtexture))
            {
                RenderFrame();
                RunRecognizer();
            }


        }
    }

    protected void RenderingNormalScene()
    {
        if (webcamtexture != null && webcamtexture.didUpdateThisFrame)
        {

            // this must be called continuously
            ReadTextureConversionParameters();

            // process texture with whatever method sub-class might have in mind
            if (ProcessTexture(webcamtexture, ref renderedtexture))
            {
                RenderFrame();
            }
        }
    }


    protected abstract void RunRecognizer();
    protected abstract bool ProcessTexture(WebCamTexture input, ref Texture2D output);

    /// <summary>
    /// Renders frame onto the surface
    /// </summary>
    private void RenderFrame()
    {
        if (webcamtexture.isPlaying)
        if (renderedtexture != null)
        {
            // apply
            displaySurface.GetComponent<UnityEngine.UI.RawImage>().texture = renderedtexture;

            // Adjust image ration according to the texture sizes 
            displaySurface.GetComponent<RectTransform>().sizeDelta = new Vector2(renderedtexture.width, renderedtexture.height);
            
        }
    }



}
