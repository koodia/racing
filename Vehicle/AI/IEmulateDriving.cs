

interface IEmulateDriving
{

	bool isVisionRange {get; set;}

	//Move the car in the level but as light as possible
	void EmulateLetAIDrive();

	//Keep certain speed
	void EmulateDriveForward();

	//Switch lane when it is compulsory, dead end or lane ends
	void EmulateLaneSwitch();

	void CheckWayPointDistance(bool laneDirectionOpposite);

	//void RenderVisualsIfVisualRange();

}
