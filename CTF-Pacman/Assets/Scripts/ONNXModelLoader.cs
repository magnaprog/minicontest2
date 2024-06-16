using UnityEngine;
using Unity.Barracuda;

public class ONNXModelLoader : MonoBehaviour
{
    public NNModel onnxModel;
    private Model runtimeModel;
    private IWorker worker;

    void Start()
    {
        // Load the ONNX model
        runtimeModel = ModelLoader.Load(onnxModel);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
    }

    public float[] Predict(float[] input)
    {
        //                       Tensor(batch, vectorlength, vector)
        Tensor inputTensor = new Tensor(1, input.Length, input);
        worker.Execute(inputTensor);
        Tensor outputTensor = worker.PeekOutput();
        float[] output = outputTensor.ToReadOnlyArray();
        // Dispose tensors
        inputTensor.Dispose();
        outputTensor.Dispose();
        return output;
    }

    void OnDestroy()
    {
        worker.Dispose();
    }
}