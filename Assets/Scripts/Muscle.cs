﻿using UnityEngine;
using System.Collections;

public class Muscle : BodyComponent {

	public MuscleJoint startingJoint;
	public MuscleJoint endingJoint;

	public Vector3 startingPoint {
		get {
			return startingJoint.position;
		}
	}

	public Vector3 endingPoint {
		get {
			return endingJoint.position;
		}	
	}

	private SpringJoint spring;

	private LineRenderer lineRenderer;

	/** Specifies whether the muscle should contract and expand or not. */
	private bool living;

	private float LINE_WIDTH = 0.5f;

	private float CONTRACTION_FACTOR = 0.2f;

	// TODO: Add rest and contraction spring tension constants

	// Use this for initialization
	public override void Start () {
		base.Start();

		highlightingShader = Shader.Find("Standard");
	}
	
	// Update is called once per frame
	void Update () {

		updateLinePoints();

		testContraction();
	}

	/** Connects the gameobject to the starting end endingJoint */
	public void connectToJoints() {

		if (startingJoint == null || endingJoint == null) return;

		startingJoint.connect(this);
		endingJoint.connect(this);

		// connect the musclejoints with a spring joint
		spring = startingJoint.gameObject.AddComponent<SpringJoint>();
		spring.spring = 1;
		spring.minDistance = 0;
		spring.maxDistance = 0;
		//spring.autoConfigureConnectedAnchor = true;
		spring.anchor = startingJoint.position;
		spring.connectedAnchor = endingJoint.position;
		spring.connectedBody = endingJoint.GetComponent<Rigidbody>();

		spring.enablePreprocessing = true;
		spring.enableCollision = false;

	}

	/** Contracts the muscle. */
	public void contract() {
		spring.spring = 100;
		StartCoroutine(ExpandAfterTime(0.5f));
	}

	/** Expands the muscle. */
	public void expand() {
		spring.spring = 1;
	}

	private void testContraction() {
		if (living) {

			StartCoroutine(ContractAfterTime(6f));
			living = false;
		}
	}

	IEnumerator ExpandAfterTime(float time)
	{
		yield return new WaitForSeconds(time);

		expand();
	}

	IEnumerator ContractAfterTime(float time) {
		yield return new WaitForSeconds(time);

		contract();

		StartCoroutine(ContractAfterTime(time));
	}

	public void addLineRenderer(){
		
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.SetWidth(LINE_WIDTH, LINE_WIDTH);
	}

	public void addCollider() {

		addColliderToLine();
	}

	private void addColliderToLine() {

		BoxCollider col = gameObject.AddComponent<BoxCollider> (); //new GameObject("Collider").

		// Collider is added as child object of line
		col.transform.parent = lineRenderer.transform;
		// length of line
		float lineLength = Vector3.Distance (startingPoint, endingPoint); 
		// size of collider is set where X is length of line, Y is width of line, Z will be set as per requirement
		col.size = new Vector3 (lineLength, LINE_WIDTH, 1f); 
		Vector3 midPoint = (startingPoint + endingPoint)/2;
		// setting position of collider object
		col.transform.position = midPoint; 
		col.center = Vector3.zero;
		// Following lines calculate the angle between startPos and endPos
		float angle = (Mathf.Abs (startingPoint.y - endingPoint.y) / Mathf.Abs (startingPoint.x - endingPoint.x));
		if((startingPoint.y < endingPoint.y && startingPoint.x > endingPoint.x) || (endingPoint.y < startingPoint.y && endingPoint.x > startingPoint.x)) {
			
			angle*=-1;
		}

		angle = Mathf.Rad2Deg * Mathf.Atan (angle);
		col.transform.Rotate (0, 0, angle);

		// Add a rigidbody
		Rigidbody rBody = gameObject.AddComponent<Rigidbody>();
		rBody.isKinematic = true;

		//lineRenderer.material.shader = Shader.Find("Standard");
		//lineRenderer.material
	}

	private void removeCollider() {
		Destroy(GetComponent<Rigidbody>());
		Destroy(GetComponent<BoxCollider>());
	}

	/** Sets the material for the attached lineRenderer. */
	public void setMaterial(Material mat) {
		lineRenderer.material = mat;
	}

	/** Points are flattened to 2D. */
	public void setLinePoints(Vector3 startingP, Vector3 endingP) {

		startingP.z = 0; 
		endingP.z = 0;

		lineRenderer.SetPositions(new Vector3[]{ startingP, endingP });
	}

	public void updateLinePoints(){

		if(startingJoint == null || endingJoint == null) return;

		setLinePoints(startingPoint, endingPoint);
	}

	public override void prepareForEvolution ()
	{
		removeCollider();
		living = true;
	}

	/** Deletes the muscle gameobject and the springjoint. */
	public override void delete() {

		Destroy(spring);
		startingJoint.disconnect(this);
		endingJoint.disconnect(this);
		Destroy(gameObject);
	}

	public void deleteWithoutDisconnecting() {

		Destroy(spring);
		Destroy(gameObject);	
	}

	public override bool Equals (object o)
	{
		return base.Equals (o) || Equals(o as Muscle);
	}

	public bool Equals(Muscle m) {

		if (m == null) return false;

		return (m.startingJoint.Equals(startingJoint) && m.endingJoint.Equals(endingJoint)) ||
			(m.startingJoint.Equals(endingJoint) && m.endingJoint.Equals(startingJoint));
	}

	private void OnDestroy()
	{
		Destroy(lineRenderer.material);
	}
}