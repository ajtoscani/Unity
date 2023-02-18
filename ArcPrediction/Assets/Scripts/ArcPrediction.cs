using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class ArcPrediction : MonoBehaviour
{
    [SerializeField]
    private LineRenderer _lineRenderer;

    [SerializeField]
    private Transform _startPos;

    [SerializeField]
    private Transform _endPos;

    [SerializeField]
    private float _arcHeight = 1f;

    [SerializeField]
    private float _speed = 10f;

    private float _distance;
    private float _stepScale;
    // Start is called before the first frame update
    void Start()
    {
        _lineRenderer.positionCount = 400;
        _distance = Vector3.Distance(_startPos.position, _endPos.position);
        _stepScale = _speed / _distance;
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        int index = 0;
        
        //Get the expected number of line points for the line renderer
        for (float _progress = 0f; _progress < 1; index++)
        {
            _progress = Mathf.Min(_progress + Time.fixedDeltaTime * _stepScale, 1.0f);
        }
        _lineRenderer.positionCount = index + 1;

        //Assigne the line points
        index = 0;
        _lineRenderer.SetPosition(index, _startPos.position);
        //The prediction uses 0 - 1 to determine when the prediction will be at the _endPos
        for (float _progress = 0f; _progress < 1;)
        {
            index++;
            _progress = Mathf.Min(_progress + Time.deltaTime * _stepScale, 1.0f);
            float parabola = 1.0f - 4.0f * (_progress - 0.5f) * (_progress - 0.5f);
            Vector3 nextPos = Vector3.Lerp(_startPos.position, _endPos.position, _progress);
            nextPos.y += parabola * _arcHeight;
            _lineRenderer.SetPosition(index, nextPos);
        }
    }
}
