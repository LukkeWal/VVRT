# Virtual Volume Ray Caster
The Virtual Ray Tracer (VRT) is an educational tool to provide users with an interactive environment for understanding ray tracing concepts. Extending VRT, we created Virtual Volume Raycaster (VVRT), an interactive and gamified application that allows to view and explore the volume raycasting process in real-time. The goal is to help the users—students of Scientific Visualization and the general public—to better understand the steps of volume raycasting and their characteristics, for example the effect of early ray termination. VVRT shows a scene containing a camera casting rays which interact with a volume in the scene. Learners are able to modify and explore different settings, e.g., concerning the transfer function or ray sampling step size. Our educational tool is built with the cross-platform engine Unity, and we make it fully available to be extended and/or adjusted to fit the requirements of courses at other institutions, educational tutorials, or of enthusiasts from the general public. Two user studies demonstrate the effectiveness of VVRT in supporting the understanding and teaching of volume raycasting.

## Using the application
To try out a ready version of the VVRT download the zip folder with [this dropbox link](https://www.dropbox.com/scl/fo/1manjxd07j5n2zjqg18ld/AMEdYlu-3ltPDNdRuyGZD6c?rlkey=a19gma37iuq2a3o3jhl0akpyo&st=vnu13trr&dl=0) which contians a build version for Windows, Mac, and Linux. Extract the zip folder, open it and launch your VVRT build. The Virtual Volume Ray Caster can be found in the levels menu under the title "Ray Casting"

Alternatively you can use the web version of VVRT in your [browser](https://lukkewal.github.io/VVRT), when using it on mobile we reccomend landscape mode.

## Building the Application
If you want to build the application yourself from the source code you can use the following steps:

As a prerequisite, you need a [Unity 6000.1.7f1](https://unity3d.com/unity/qa/lts-releases) release. 

- open the `UnityProject` folder with Unity [Unity 6000.1.7f1](https://unity3d.com/unity/qa/lts-releases)
- naviate to `CreateVoxelGrid > 3DTexture` and click the 3DTexture button to generate the 3D textures
- navigate to `File > Build Settings`, select your desired platform and press 'build'. The application has been tested on Windows, Linux and WebGL
For more information on building Unity applications see the [Unity Manual page](https://docs.unity3d.com/Manual/BuildSettings.html).

## About us

Virtual Ray Tracer (VRT) as well as Virtual Volume Raycaster (VVRT) were created by students during projects for their Computing Science degree programme at the University of Groningen. The projects were proposed and supervised by Jiri Kosinka and Steffen Frey in the SVCG research group. The application was built to aid students of the Computer Graphics or Scientific Visualization courses at the University of Groningen by providing them with an interactive introduction to the principles of ray tracing and ray casting.

## Future

The application is under active development. Additional features as well as software maintenance are being worked on.

## Papers

##### Papers:

[C.S. van Wezel, W.A. Verschoore de la Houssaije, S. Frey, and J. Kosinka: Virtual Ray Tracer 2.0, Special Section on EG2022 Edu Best Papers.](https://doi.org/10.1016/j.cag.2023.01.005)

[W.A. Verschoore de la Houssaije, C.S. van Wezel, S. Frey, and J. Kosinka: Virtual Ray Tracer, Eurographics 2022 Education Papers.](https://diglib.eg.org/handle/10.2312/eged20221045)

[L. v.d. Wal, P. Blesinger, J. Kosinka , and S. Frey: VVRT: Virtual Volume Raycaster, Eurovis 2025 Education Papers.](https://diglib.eg.org/bitstream/handle/10.2312/eved20251021/eved20251021.pdf)

##### Theses:

[W.A. Verschoore de la Houssaije: A Virtual Ray Tracer, BSc thesis, University of Groningen, 2021.](http://fse.studenttheses.ub.rug.nl/24859)

[C.S. van Wezel: A Virtual Ray Tracer, BSc thesis, University of Groningen, 2022.](http://fse.studenttheses.ub.rug.nl/26455)

[J. van der Zwaag: Virtual Ray Tracer: Distribution Ray Tracing, BSc Thesis, University of Groningen, 2022.](https://fse.studenttheses.ub.rug.nl/27881)

[B. Yilmaz: Acceleration data structures for Virtual Ray Tracer, BSc Thesis, University of Groningen, 2022.](https://fse.studenttheses.ub.rug.nl/27838)

[PJ. Blok: Gamification of Virtual Ray Tracer, BSc Thesis, University of Groningen, 2022.](https://fse.studenttheses.ub.rug.nl/27596)

[R. Rosema: Adapting Virtual Ray Tracer to a Web and Mobile Application, BSc Thesis, University of Groningen, 2022.](https://fse.studenttheses.ub.rug.nl/27894)

[T. Couperus: Virtual Ray Tracer: Teaching Procedural Textures, BSc Thesis, University of Groningen, 2023.](https://fse.studenttheses.ub.rug.nl/30703/)

[V. Gaya Walters: Raster Textures in Virtual Ray Tracer, BSc Thesis, University of Groningen, 2023.](https://fse.studenttheses.ub.rug.nl/30801/)

[L. van der Wal: Virtual Ray Tracer: Ray Casting Support, BSc Thesis, University of Groningen, 2023.](https://fse.studenttheses.ub.rug.nl/31739/)

[P. Blesinger: Real-time Visualisation of Volume Raycasting in Virtual Ray Tracer. Bachelor's Thesis, University of Groningen 2024](https://fse.studenttheses.ub.rug.nl/34382/)

[A. Aaen: Virtual Ray Tracer in VR, BSc thesis, University of Groningen, 2024.](https://fse.studenttheses.ub.rug.nl/33355/)

[I, Bodola: Virtual Ray Tracer: Using Light to Deform Objects, BSc thesis, University of Groningen, 2024.](https://fse.studenttheses.ub.rug.nl/33891/)

##### Others:

[A, Bredenbals: Visualising Ray Marching in 3D, Msc Research Internship, University of Groningen, 2022.](https://fse.studenttheses.ub.rug.nl/27977/)

Further documents will appear here in due course.



## License

The application is released under the MIT license. Therefore, you may use and modify the code as you see fit. If you use or build on the application we would appreciate it if you cited this repository and the Eurographics 2022 paper. As we are still working on the application ourselves, we would also like to hear about any improvements you may have made.

## Contact

Any questions, bug reports or suggestions can be created as an issue on this repository. Alternatively, please contact [Jiri Kosinka](http://www.cs.rug.nl/svcg/People/JiriKosinka).
