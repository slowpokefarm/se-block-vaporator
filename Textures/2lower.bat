for /R %%f in (*.DDS) do (

	ren %%~pf\%%~nf.DDS %%~nf.dds
)
pause