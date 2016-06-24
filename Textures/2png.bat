for /R %%f in (*.dds) do (

	D:\Projects\SE\texconv\texconv -nologo -ft tga %%~ff
)
pause