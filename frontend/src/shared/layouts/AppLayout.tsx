import { NavLink, Outlet, useNavigate } from "react-router-dom";
import { FileText, LayoutDashboard, ClipboardCheck, Users, Bell, ShieldCheck, LogOut } from "lucide-react";
import { cn } from "@/shared/lib/utils";
import { limparSessao } from "@/shared/lib/api-client";
import { Button } from "@/shared/components/ui/button";

/**
 * Layout do app autenticado: sidebar fixa com um item por módulo
 * (docs/05-estrutura-de-pastas.md - um por feature folder), e um <Outlet />
 * para a página da rota atual. O controle de QUAIS itens aparecem por papel
 * do usuário (ex.: "Auditoria" só para AdminEmpresa) será refinado quando o
 * hook useAuth (feature auth) estiver disponível - por ora a sidebar mostra
 * todos os itens, e a proteção de fato acontece no backend (RN18/RN32) e em
 * RotaProtegida.
 */
const itensNavegacao = [
  { rota: "/documentos", rotulo: "Documentos", icone: FileText },
  { rota: "/revisao", rotulo: "Fila de Revisão", icone: ClipboardCheck },
  { rota: "/dashboard", rotulo: "Dashboard", icone: LayoutDashboard },
  { rota: "/administracao/usuarios", rotulo: "Usuários", icone: Users },
  { rota: "/notificacoes", rotulo: "Notificações", icone: Bell },
  { rota: "/auditoria", rotulo: "Auditoria", icone: ShieldCheck }
];

export function AppLayout() {
  const navigate = useNavigate();

  function handleLogout() {
    limparSessao();
    navigate("/login", { replace: true });
  }

  return (
    <div className="flex min-h-screen">
      <aside className="flex w-64 flex-col border-r bg-card">
        <div className="border-b p-4">
          <span className="text-lg font-bold tracking-tight">IntelliDoc</span>
        </div>

        <nav className="flex-1 space-y-1 p-3">
          {itensNavegacao.map(({ rota, rotulo, icone: Icone }) => (
            <NavLink
              key={rota}
              to={rota}
              className={({ isActive }) =>
                cn(
                  "flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors",
                  isActive
                    ? "bg-primary text-primary-foreground"
                    : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
                )
              }
            >
              <Icone className="h-4 w-4" />
              {rotulo}
            </NavLink>
          ))}
        </nav>

        <div className="border-t p-3">
          <Button variant="ghost" className="w-full justify-start gap-3" onClick={handleLogout}>
            <LogOut className="h-4 w-4" />
            Sair
          </Button>
        </div>
      </aside>

      <main className="flex-1 overflow-y-auto bg-muted/20 p-6">
        <Outlet />
      </main>
    </div>
  );
}