function [y] = graficarBlack(x,y,z,tiempo,a,b)

    disp(a);
    disp(b);
    y = plot(tiempo(b:a),x(b:a),'k',tiempo(b:a),y(b:a),'k',tiempo(b:a),z(b:a),'k');
end



