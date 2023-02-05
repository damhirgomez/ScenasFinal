function  [Corr_APR_IMU]=Correlacion(dato1,dato2)
[C21,lag21] = xcorr(dato1,dato2);
C21 = C21/max(C21);
[M21,I21] = max(C21);
Corr_APR_IMU = lag21(I21);


end

