function [y] = graficarBlack(x,y,z,tiempo,a,b)

    y = plot(tiempo(b:a),x(b:a),'k',tiempo(b:a),y(b:a),'k',tiempo(b:a),z(b:a),'k');
end



