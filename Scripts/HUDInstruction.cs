using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;
public class HUDInstruction : MonoBehaviour {

	private const string referenceError = "Please reference Unity GUI Text and Raw Image Components to HUDInstruction instance!";

	private static HUDInstruction _instance;
	public static HUDInstruction Instance
	{
		get {
			if (!_instance)
				return _instance = new GameObject().AddComponent<HUDInstruction>();
			else 
				return _instance;
		}
	}

	public InstructionSet currentInstructionSet;
	public IDictionary<string, InstructionSet> KnownSets = new SortedDictionary<string, InstructionSet>();
	
	public Text InstructionTextBesideImage;
	
	public Text InstructionTextWide;

	public RawImage InstructionImage;
    
	bool SwitchToNextInstruction = false;
    bool forceStop = false;

	void Awake()
	{
		_instance = this;
	}

	void Start () {

		if (!InstructionImage || 
			!InstructionTextBesideImage || 
			!InstructionTextWide)
		{
			throw new MissingReferenceException(referenceError);		
		} 
	}
	 

	public void StartDisplaying(string nameOfInstructionSet)
	{
		if (!KnownSets.ContainsKey(nameOfInstructionSet)) {
			throw new ArgumentException("The requested instruction set is not available! Please load it or create it.");
		}

		currentInstructionSet = KnownSets[nameOfInstructionSet];

		ActivateRendering();

		StartCoroutine(StartInstructionRendering());
	}

	void ActivateRendering()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).gameObject.SetActive(true);
		}
	}

	public void StartDisplaying(InstructionSet set)
	{
		currentInstructionSet = set;
		ActivateRendering();
		StartCoroutine(StartInstructionRendering());
	}

	public void StartDisplaying(Instruction instruction)
	{
		currentInstructionSet = new InstructionSet();
		currentInstructionSet.instructions.AddLast(instruction);

		ActivateRendering();
		StartCoroutine(StartInstructionRendering());
	}

    public void StopDisplaying()
    {
        forceStop = true;
    }

    public void SkipCurrent()
    {
        SwitchToNextInstruction = true;
    }

	IEnumerator StartInstructionRendering()
	{
		IEnumerator<Instruction> instructionEnumerator = currentInstructionSet.instructions.GetEnumerator();

		while (instructionEnumerator.MoveNext() && !forceStop)
		{
			var instruction = instructionEnumerator.Current;

			Display(instruction);

            if (instruction.DisplayTime == 0 && SwitchToNextInstruction)
            {
                SwitchToNextInstruction = false;
				yield return new WaitForEndOfFrame();
			} 
			   
			yield return new WaitForSeconds(instruction.DisplayTime);
		}

        forceStop = false;
        
        currentInstructionSet.instructions.Clear();

		HideInstructionHUD();

		yield return null;
	}
	
	void Display(Instruction instruction){

		InstructionTextBesideImage.text = string.Empty;
		InstructionTextWide.text = instruction.Text;
		InstructionImage.gameObject.SetActive(false); 
	}

	void Display(InstructionWithImage instruction)
	{
		InstructionTextBesideImage.text = instruction.Text;
		InstructionTextWide.text = string.Empty;
		InstructionImage.gameObject.SetActive(true);
		
		if (instruction.ImageTexture != null) { 
			InstructionImage.texture = instruction.ImageTexture;
		}
	}

	void Display(GameObject aVisibleObject)
	{
		InstructionTextBesideImage.gameObject.SetActive(false);
		InstructionTextWide.gameObject.SetActive(false);
		InstructionImage.gameObject.SetActive(false);

	}

	void HideInstructionHUD()
	{
		Debug.Log("Calling HideInstructionHUD");
		
		for (int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).gameObject.SetActive(false);    
		}
	}

	#region TODO Fluent interface 
	// Idea of building a Factory with fluent interface pattern

	//public HUDInstruction Give(string instruction)
	//{
	//    if (InstructionTextBesideImage)
	//        InstructionTextBesideImage.textFromSomewhere = instruction;

	//    return this;
	//}

	//public HUDInstruction With(Texture image)
	//{
	//    if (InstructionImage)
	//        InstructionImage.texture = image;

	//    return this;
	//}

	//public HUDInstruction For(float presentationTime)
	//{
	//    this.presentationTime = presentationTime; 
	//    return this;
	//}

	//public void Close()
	//{
	//    InstructionImage.texture = null;
	//    InstructionTextBesideImage.text = string.Empty;
	//}
#endregion
}

public class InstructionFactory
{ 
	public static InstructionSet ReadInstructionSetFrom(string fileName){

		var lines = File.ReadAllLines(fileName, System.Text.Encoding.UTF8);

		var resultSet = new InstructionSet();

		resultSet.identifier = Path.GetFileNameWithoutExtension(fileName);

		Process(lines.ToList(), resultSet);

		return resultSet;
	}
	 
	public static void Process(List<string> lines, InstructionSet set)
	{
		if (!lines.Any())
			return;

		string head = lines.First();

		if (head.ContainsTextEndingWithLineBreak())
		{
			set.instructions.AddLast(new Instruction());
			set.instructions.Last.Value.Text = head; 
		}

		if (Regex.IsMatch(head, @"^t:[0-9]+$"))
		{
			string timeDesc = head;
			string timeValue = timeDesc.Substring(2);
			float time = 0f;

			if (!float.TryParse(timeValue, out time))
				Debug.LogError("Could not parse time value for instruction using 0!");

			set.instructions.Last.Value.DisplayTime = time;

		}

		if (Regex.IsMatch(head, @"^i:[0-9]+[.]?[a-zA-Z]+$"))
		{
			string imageReference = lines.First();
			string imageName = imageReference.Substring(2); 
			 
			var temp = set.instructions.Last.Value;
			set.instructions.RemoveLast();

			var instructionContainingImage = new InstructionWithImage();
			instructionContainingImage.Text = temp.Text;
			instructionContainingImage.DisplayTime = temp.DisplayTime;
			instructionContainingImage.ImageName = imageName;

			set.instructions.AddLast(instructionContainingImage);
		}
		
		lines.Remove(head);

		Process(lines, set);
	}

	public static void SaveTo(InstructionSet instructions, string fileName)
	{
		throw new NotImplementedException("currently unimportant feature");
		// TODO
		// File.WriteAllLines(fileName, lines);
	}

}

public class InstructionSet 
{
	public string identifier = string.Empty;

	public LinkedList<Instruction> instructions = new LinkedList<Instruction>();
	 
}

public class Instruction
{
	public string Text;

	public float DisplayTime;

	public Instruction()
	{
		Text = "Fizz";
		DisplayTime = 0f;
	}

	public Instruction(string textFromSomewhere, float time)
	{
		this.Text = textFromSomewhere;
		this.DisplayTime = time;
	}
}

public class InstructionWithImage : Instruction
{
	public string ImageName;

	public Texture ImageTexture;
}
 
public static class StringExtensions
{
	public static bool ContainsTextEndingWithLineBreak(this string line)
	{
		return Regex.IsMatch(line, @"^[^it:][a-zA-Z0-9!? ,#.;,':%&§|><()\[\]{}=ßÄÖÜäöü]+$");
	}
}