using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine.AI;
using UnityEngine.SceneManagement;


public class MapGenerator : MonoBehaviour {

	public static MapGenerator current;


	int height;
	int width;




	public TileBase shallowSea;
	public List<TileBase> coastSea;
	public List<TileBase> rockGrass;
	public List<TileBase> rockCoast;
	public List<TileBase> grassCoast;
	public List<TileBase> grassShallow;
	public List<TileBase> rockShallow;
	public List<TileBase> deepSea;


	public GameObject oakTree;

	public GameObject oliveTree;
	public GameObject rockBig;


	public List<float> randomList;
	public int randomIndex=0;

	public Tilemap tilemap;


	Camera m_MainCamera;



	int randomFillPercent;

	// two dimensional array; rows and columns
	public int[,] map;


	void Awake(){
		current = this;

		//the following numbers are the best dimensions for the script. Of course you can change them but then you need to update some numbers , e.g. m_MainCamera.orthographicSize
		height = 300;
		width = 300;

		//if the following number is updated as smaller than the specified, then there will be many smaller islands generated in the sea.
		randomFillPercent = 46;

	}



	void Start(){
		GenerateMap ();
		m_MainCamera = Camera.main;
		m_MainCamera.orthographicSize = 740;
		m_MainCamera.transform.position = new Vector3(740,740,-10);

		randomIndex=0;


	}


	void Update()
	{
		Quit ();
	}

	private void Quit() {
		if (Input.GetKeyDown(KeyCode.Q))
			SceneManager.LoadScene ("MainScene");
	}


	public void GenerateMap() {
		
		map = new int[width, height];
		string seed = GenerationScreen.seed;

		randomList = RandomFillMap (seed);


		for (int i = 0; i < 4; i ++){
			SmoothMap();
		}




		ProcessMap ();

		GetCoastTile ();

		AddOaksOlivesRocks (randomList);


		DrawMap ();


	}



	// this method is to define the sea, the lake and the islands
	void ProcessMap() {

		List<List<Coord>> wallRegions = GetRegions(0);

		int lakeThresholdSize = 200;
		int seaThresholdSize = 3000;
		int wallThresholdSize = 400;

		foreach (List<Coord> wallRegion in wallRegions) {
			if (wallRegion.Count < wallThresholdSize) {
				foreach (Coord tile in wallRegion) {
					map [tile.tileX, tile.tileY] = 1;
				}
			}

		}


		List<List<Coord>> seaRegions = GetRegions(1);

		foreach (List<Coord> seaRegion in seaRegions) {
			if (seaRegion.Count < lakeThresholdSize) {
				foreach (Coord tile in seaRegion) {
					map [tile.tileX, tile.tileY] = 0;
				}
			} else if (seaRegion.Count > lakeThresholdSize && seaRegion.Count <= seaThresholdSize) {
				foreach (Coord tile in seaRegion) {
					map [tile.tileX, tile.tileY] = 3;
				}
			} else if (seaRegion.Count > seaThresholdSize) {
				foreach (Coord tile in seaRegion) {
					map [tile.tileX, tile.tileY] = 4;
				}
			}
		}


	}





	//the following method calculates the oliveTree, the oakTree and the rock tiles acoording to the seed generated.
	void AddOaksOlivesRocks(List<float> r){

		List<List<Coord>> wallRegions = GetRegions(0);

		int i;

		List<int> type = new List<int> ();

		foreach (var rand in r) {
			if (rand <= 25) {
				i = 2;
			} else if (rand > 25 && rand <= 50) {
				i = 6;
			} else if (rand > 50 && rand <= 75) {
				i = 9;
			} else {
				i = 7;
			}
			type.Add (i);
		}


		foreach (var wall in wallRegions) {
			int h = 0;
			foreach (var w in wall) {

				if (map [w.tileX, w.tileY] == 0 && w.corner == false) {
					if (r [h] < 70) {
						map [w.tileX, w.tileY] = type [w.xRegion * 30 + w.yRegion];
					} else {
						if (type [w.xRegion * 30 + w.yRegion] == 7) {
							if (r [h] < 80) {
								map [w.tileX, w.tileY] = 6;
							} 

						} else {
							if (type [w.xRegion * 30 + w.yRegion] == 2 && r [h] < 90) {
								map [w.tileX, w.tileY] = 9;
							} else {
								map [w.tileX, w.tileY] = 7;
							}
						}

					}
			
					h++;
				} 
				if (map [w.tileX, w.tileY] == 0 && w.corner == true){
					if (r [h] < 7 ){
						map [w.tileX, w.tileY] = type [w.xRegionNext * 30 + w.yRegion];

					}
				}
					
	
			}
		}



	}



	//to define the coast tles according to the neighbourhood to the sea tiles
	void GetCoastTile(){

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (map [x, y] == 0) {
					Coord coast = new Coord (x, y);
					bool isCoast = false;
					List<Coord> coastNei = GetNeighbours (coast);
					foreach (var c in coastNei) {
						if (map [c.tileX, c.tileY] == 4 || map [c.tileX, c.tileY] == 5) {
							isCoast = true;
							map [c.tileX, c.tileY] = 5;
							Coord shallowSea = new Coord (x, y);
							List<Coord> shallowN = GetNeighbours (shallowSea);
							foreach (var s in shallowN) {
								if (map [s.tileX, s.tileY] == 4) {
									map [s.tileX, s.tileY] = 5;
								}
							}
						}
					}

					if (isCoast == true) {
						map [x, y] = 8;
					}

				}


			}
		}

	}



	//the following method returns a value according to the adjacents tiles to suggest the correct tile to place in DrawMap().
	int CheckAdjacents (Coord n, int mapVal){
		
		int index = 0;

		if ( IsInMapRange (n.tileX - 1, n.tileY) && map [n.tileX - 1, n.tileY] == mapVal) {
			index = 1 + index;
		} if ( IsInMapRange (n.tileX, n.tileY - 1) && map [n.tileX, n.tileY - 1] == mapVal) {
			index = 10 + index;
		} if (IsInMapRange (n.tileX + 1, n.tileY) && map [n.tileX + 1, n.tileY] == mapVal) {
			index = 100 + index;
		} if (IsInMapRange (n.tileX, n.tileY + 1) && map [n.tileX, n.tileY + 1] == mapVal) {
			index = 1000 + index;
		}

		if (index == 1)
			return 0;
		else if (index == 10)
			return 3;
		else if (index == 100)
			return 2;
		else if (index == 1000)
			return 1;
		else if (index == 11)
			return 7;
		else if (index == 110)
			return 6;
		else if (index == 1100)
			return 5;
		else if (index == 1001)
			return 4;
		else if (index == 1110)
			return 8;
		else if (index == 1101)
			return 11;
		else if (index == 1011)
			return 10;
		else if (index == 111)
			return 9;
		else if (index == 1010)
			return 13;
		else if (index == 101)
			return 13;
		else if (index == 1111)
			return 12;
		else if (index == 0)
			return 13;
		else
			return 13;

	}


	bool IsInMapRange (int x, int y){
		return x >= 0 && x < width && y >= 0 && y < height;
	}

	// this method is to fill the map randomly with 0 and 1.
	List<float> RandomFillMap(string seed) {
		List<float> temp = new List<float>();

		//pseudo random number generator
		System.Random pseudoRandom = new System.Random(seed.GetHashCode());

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				//to encourage the algorith to set 1 for the edges of the map: as water (wall)
				if (x < 20 || x > width - 30 || y < 30 || y > height - 30) {
					map [x, y] = 1;
				} 
				else { 
					// following to generate a random number between 0 and 100. if the number is smaller than the 'randomFillPercent' map[x,y] = 1 , else map[x,y] = 0
					int random = pseudoRandom.Next (0, 100);
					map[x,y] = (random < randomFillPercent) ? 1 : 0;
					if (x > width - 100){
						if (random > 93 && map [x, y] == 0) {
							map [x, y] = 1;
						}
					} else if (y > height - 100){
						if (random > 94 && map [x, y] == 0) {
							map [x, y] = 1;
						}
					}
					else if (x > width - 130 && y > height - 130 ) {
						if (random > 75 && map [x, y] == 0) {
							map [x, y] = 1;
						}
					} 
					else if (x < 130 && y < 100){
						if (random > 95 && map [x, y] == 0) {
							map [x, y] = 1;
						}
					} 
			
					temp.Add(random);

				}
			}
		}
		return temp;
	}



	//to process 1,0 values in the map to have a appropriate shape for the island
	void SmoothMap() {

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				int neighbourWallTiles = GetSurrendingWallCount (x, y);

				if (neighbourWallTiles > 4) {
					map [x, y] = 1;
				} else if (neighbourWallTiles < 4) {
					map [x, y] = 0;
				}

			}
		}



	}




	//the following method to get a list of the same tile groups
	List<List<Coord>> GetRegions(int tileType){
		List<List<Coord>> regions = new List<List<Coord>> ();
		int[,] mapFlags = new int[width, height];

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				if (mapFlags [x, y] == 0 && map [x, y] == tileType) {
					List<Coord> newRegion = GetRegionTiles (x, y);
					regions.Add (newRegion);

					foreach (Coord tile in newRegion) {
						mapFlags [tile.tileX, tile.tileY] = 1;
					}
				}
			}
		}
		return regions;
	}

	//the following method detects the same tile group and add their coordinates to a list
	List<Coord> GetRegionTiles(int startX, int startY){
		List<Coord> tiles = new List<Coord> ();
		int[,] mapFlags = new int[width, height];
		int tileType = map [startX, startY];

		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (new Coord (startX, startY));
		mapFlags [startX, startY] = 1;

		while (queue.Count > 0) {
			Coord tile = queue.Dequeue ();
			tiles.Add (tile);

			for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++) {
				for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++) {
					if (IsInMapRange (x, y) && (y == tile.tileY || x == tile.tileX)) {
						if(mapFlags[x,y] == 0 && map[x,y] == tileType){
							mapFlags [x, y] = 1;
							queue.Enqueue (new Coord (x, y));
						}
					}
				}
			}
		}

		return tiles;

	}

	//how many neighbouring tiles which are the same does this tile have. this method returns the neighbour tiles sum value to smooth map method.
	int GetSurrendingWallCount(int gridX, int gridY){
		int wallCount = 0;
		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) {
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++) {
				//to check the grid is inside the outer bounder. if not, we add +1 to the wallcount to encourage the walls at the edge of the map (blacks in the map).
				if (IsInMapRange(neighbourX, neighbourY)) {
					if (neighbourX != gridX || neighbourY != gridY) {
						// if the neighbourX is not the gridX self or the neighbourY is not the gridY self, then get their map[x,y] value and add to the wallCount - ref. SmoothMap()
			
						wallCount += map[neighbourX, neighbourY];
					}
				} 
				else {
					// if neighbourX, neighbourY are not in the map range, then we encourage water=wall
					wallCount++;
				}
			}
		}
		return wallCount;
	}


	public struct Coord {
		public int tileX;
		public int tileY;
		public int xRegion;
		public int xRegionNext;
		public int yRegion;
		public bool corner;
		public int size;

		//the following definitions are later used to define oakTree, oliveTree and rock regions. The regions do not have natural shapes though..
		public Coord(int x, int y){
			tileX = x;
			tileY = y;
			xRegion = Mathf.RoundToInt(tileX / 20);
			xRegionNext = xRegion + 1;
			yRegion = Mathf.RoundToInt(tileY / 20);
			size = (xRegion*30 +yRegion) % 3;

			if(size!=0){
				if ((tileX % (20/size) < 4 /size*2 || tileX % (20/size) > 16/size) && (tileY % (20/size) < 4/size*2 || tileY % (20/size) > 16/size)){
					corner = true;
				} else {

					corner = false;

				}

			} else {
				if ((tileX % 20 < 4  || tileX % 20 > 16) && (tileY % 10 < 3 || tileY % 10 > 7)){
					corner = true;
				} else {
					corner = false;
				}
			}
		}



	}


	//to replace the tiles, the oliveTrees, the oakTrees, the rocks to the tilemap
	void DrawMap(){

		int shallowNumber = 0;
		for (int x = 0; x < map.GetLength (0)-1; x++) {
			for (int y = 0; y < map.GetLength (1)-1; y++) {
				Vector3Int position = new Vector3Int (x,y,0);
				Vector3 itemPos = tilemap.GetCellCenterWorld (position);
				Coord t = new Coord (x,y);

				if (map [x, y] == 7 || map [x, y] == 6 || map [x, y] == 9 || map [x, y] == 0 || map [x, y] == 13 || map [x, y] == 14) {



					if (map [x, y] == 6) { 

						Instantiate (oakTree, new Vector3 (itemPos.x + UnityEngine.Random.Range (-1f, 1f), itemPos.y + UnityEngine.Random.Range (-1f, 1f), 0), Quaternion.identity);

						if (randomList [y] < 30) {
							oakTree.transform.localScale = new Vector3 (1.5F, 1.5F, 0);

						} else if (randomList [y] >= 30 && randomList [y] < 60) {
							oakTree.transform.localScale = new Vector3 (1F, 1.3F, 0);

						} else if (randomList [y] >= 60 && randomList [y] < 80) {
							oakTree.transform.localScale = new Vector3 (1F, 1F, 0);

						} 








					} else if (map [x, y] == 9) { 


						Instantiate (oliveTree, new Vector3 (itemPos.x + UnityEngine.Random.Range (-1f, 1f), itemPos.y + UnityEngine.Random.Range (-1f, 1f), 0), Quaternion.identity);

						if (randomList [y] < 30) {
							oliveTree.transform.localScale = new Vector3 (1.2F, 1F, 0);

						} else if (randomList [y] >= 30 && randomList [y] < 60) {
							oliveTree.transform.localScale = new Vector3 (1F, 1.3F, 0);

						} else if (randomList [y] >= 60 && randomList [y] < 80) {
							oliveTree.transform.localScale = new Vector3 (1F, 1F, 0);

						} 

						if (randomList [y] < 50) {
							oliveTree.GetComponent<SpriteRenderer> ().flipX = true;

						} else {
							oliveTree.GetComponent<SpriteRenderer> ().flipX = false;


						}




					}



					int z = CheckAdjacents (t, 8);

					int f = CheckAdjacents (t, 3);

					if (z != 13) {
						tilemap.SetTile (position, grassCoast[z]);
					} else {
						if (f != 13) {
							tilemap.SetTile (position, grassShallow [f]);
						} else {
							tilemap.SetTile (position, grassShallow[13]);
						}
					}


				}
				if (map [x, y] == 3) {
					tilemap.SetTile (position, shallowSea);

				}
				if (map [x, y] == 8) {


					int i = CheckAdjacents (t, 5);

					tilemap.SetTile (position, coastSea [i]);

				}
				if (map [x, y] == 4|| map [x, y] == 1) {




					int i = CheckAdjacents (t, 5);
					tilemap.SetTile (position, deepSea [i]);

				}
				if (map [x, y] == 5) {
					shallowNumber++;
					tilemap.SetTile (position, shallowSea);




				}
				if (map [x, y] == 2) {
		

					for (int m = 6; m < 10; m++) {
						int i = CheckAdjacents (t, m);
						if (i != 13) {
							if (m == 8) {
								tilemap.SetTile (position, rockCoast [i]);
							} else {
								tilemap.SetTile (position, rockGrass [i]);
							}
							break;
						} else {
							int q = CheckAdjacents (t, 3);
							if (q != 13) {
								tilemap.SetTile (position, rockShallow [q]);
								break;
							} else {
								int p = CheckAdjacents (t, 0);
								if (p != 13) {
									tilemap.SetTile (position, rockGrass [p]);
									break;
								} else {
									tilemap.SetTile (position, rockGrass [13]);

								}

							}
						}

					}






					Instantiate (rockBig, new Vector3 (itemPos.x + UnityEngine.Random.Range (-1.5f, 1.5f), itemPos.y + UnityEngine.Random.Range (-1.5f, 1.5f), 0), Quaternion.identity);


					if (randomList [y] < 30) {
						rockBig.transform.localScale = new Vector3 (1F, 1F, 0);

					} else if (randomList [y] >= 30 && randomList [y] < 60) {
						rockBig.transform.localScale = new Vector3 (1F, 1.3F, 0);

					} else if (randomList [y] >= 60 && randomList [y] < 80) {
						rockBig.transform.localScale = new Vector3 (1.5F, 1.5F, 0);

					}






				}




			}
		}






	}


	//the following method adds the neighbour tiles to a list
	public List<Coord> GetNeighbours(Coord co){
		List<Coord> neighbours = new List<Coord> ();

		for (int x = -1;x <= 1; x++){
			for (int y = -1;y <= 1; y++){
				if (x == 0 && y == 0) {
					continue;
				} else {
					int neighbourX = co.tileX + x;
					int neighbourY = co.tileY + y;

					neighbours.Add (new Coord(neighbourX, neighbourY));

				}
			}
		}
		return neighbours;

	}



}
