for /R %%f in (*.tga) do (

	D:\Projects\SE\texconv\texconv -f BC7_UNORM -sepalpha %%~ff
)
pause