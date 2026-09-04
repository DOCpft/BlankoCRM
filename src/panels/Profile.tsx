import { FC } from 'react';
import { NavIdProps, Panel, PanelHeader, Group, Header, Div, Placeholder } from '@vkontakte/vkui';

export interface ProfileProps extends NavIdProps {}

export const Profile: FC<ProfileProps> = ({ id }) => {
  return (
    <Panel id={id}>
      <PanelHeader>
        <div style={{ width: '100%', textAlign: 'center' }}>Профиль</div>
      </PanelHeader>
      <Group header={<Header>Мастер</Header>}>
        <Div>
          <Placeholder>Информация о мастере и настройки аккаунта.</Placeholder>
        </Div>
      </Group>
    </Panel>
  );
};

export default Profile;
